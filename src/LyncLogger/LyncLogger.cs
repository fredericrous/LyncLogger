using System;
using System.Linq;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using Microsoft.Lync.Model.Conversation.AudioVideo;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;

namespace LyncLogger
{
    class LyncLogger
    {
        private static string LOG_HEADER = "// Convestation started with {0} on {1}"; //header of the file
        private static string LOG_MIDDLE_HEADER = "---- conversation resumed ----"; //middle header of the file
        private static string LOG_MESSAGE = "{0} ({1}): {2}"; //msg formating
        private static string LOG_AUDIO = "Audio conversation {0} at {1}"; //msg audio started/ended formating

        private static int DELAY_RETRY_AUTHENTICATION = 20000; // delay before authentication retry (in ms)
        private static string EXCEPTION_LYNC_NOCLIENT = "The host process is not running";

        private static DirectoryInfo _folderLog; 
        private static string _fileLog;

        public LyncLogger(string folderLog)
        {
            _folderLog = new DirectoryInfo(folderLog);
            _fileLog = Path.Combine(folderLog, "conversation_{0}_{1}.log");

            run();
        }

        /// <summary>
        /// Constructor, Listen on new openned conversations
        /// </summary>
        /// <param name="folderLog">folder to log conversation files</param>
        public void run()
        {
            try
            {
                //Start the conversation
                LyncClient client = LyncClient.GetClient();


                //handles the states of the logger displayed in the systray
                client.StateChanged += (s, e) =>
                {
                    if (e.NewState == ClientState.SignedOut)
                    {
                        NotifyIconSystray.ChangeLoggerStatus(false);
                        run();
                    }
                };

                if (client.State == ClientState.SignedIn) 
                {
                    //listen on conversation in order to log messages
                    ConversationManager conversations = client.ConversationManager;

                    //check our listener hasn't 
                    var handler = typeof(ConversationManager).GetField("ConversationAdded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(conversations) as Delegate;

                    if (handler == null)
                    {
                        conversations.ConversationAdded += conversations_ConversationAdded;
                        NotifyIconSystray.ChangeLoggerStatus(true);
                    }
                   
                }
                else
                {
                    Thread.Sleep(DELAY_RETRY_AUTHENTICATION / 10);
                    run();
                }
                
            }
            catch (LyncClientException lyncClientException)
            {
                Console.Out.WriteLine(lyncClientException);
                if (lyncClientException.Message.Equals(EXCEPTION_LYNC_NOCLIENT))
                {
                    Thread.Sleep(DELAY_RETRY_AUTHENTICATION);
                    run();
                }
            }
            catch (SystemException systemException)
            {
                if (IsLyncException(systemException))
                {
                    // Log the exception thrown by the Lync Model API.
                    Console.WriteLine("Error: " + systemException);
                }
                else
                {
                    // Rethrow the SystemException which did not come from the Lync Model API.
                    throw;
                }
            }
        }

        /// <summary>
        /// Create conversation log file and listen on what participants say
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void conversations_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            String firstContactName = e.Conversation.Participants.Count > 1
            ? e.Conversation.Participants[1].Contact.GetContactInformation(ContactInformationType.DisplayName).ToString()
            : "meet now";
            DateTime currentTime = DateTime.Now;

            String fileLog = String.Format(_fileLog, firstContactName.Replace(", ", "_"), currentTime.ToString("yyyyMMdd"));

            String logHeader;
            FileInfo[] Files = _folderLog.GetFiles("*.log");
            if (Files.Count(f => f.Name == fileLog.Substring(fileLog.LastIndexOf('\\') + 1)) == 0)
            {
                logHeader = String.Format(LOG_HEADER, firstContactName, currentTime.ToString("yyyy/MM/dd"));
            }
            else
            {
                logHeader = String.Format(LOG_MIDDLE_HEADER);
            }

            using (FileStream stream = File.Open(fileLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(logHeader);
                }
            }

            Conversation conv = e.Conversation;

            //detect all participant (including user)
            conv.ParticipantAdded += (_sender, _e) =>
            {
                var participant = _e.Participant;
                InstantMessageModality remoteImModality = (InstantMessageModality)participant.Modalities[ModalityTypes.InstantMessage];

                //detect all messages (including user's)
                remoteImModality.InstantMessageReceived += (__sender, __e) =>
                {
                    remoteImModality_InstantMessageReceived(__sender, __e, fileLog);
                };
            };

            //get audio conversation informations about user (not the other participants)
            AVModality callImModality = (AVModality)conv.Participants[0].Modalities[ModalityTypes.AudioVideo];
            //notify call 
            callImModality.ModalityStateChanged += (_sender, _e) =>
            {
                callImModality_ModalityStateChanged(_e, fileLog);
            };
        }

        /// <summary>
        /// log to fileLog the date of the start and end of a call
        /// (ModalityStateChanged callback)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="fileLog"></param>
        static void callImModality_ModalityStateChanged(ModalityStateChangedEventArgs e, String fileLog)
        {
            //write log only on connection or disconnection
            if (e.NewState == ModalityState.Connected || e.NewState == ModalityState.Disconnected)
            {
                //write start/end info to log
                using (FileStream stream = File.Open(fileLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(String.Format(LOG_AUDIO,
                            (e.NewState == ModalityState.Connected) ? "started" : "ended",
                            DateTime.Now.ToString("HH:mm:ss")
                        ));
                    }
                }
            }
        }

        /// <summary>
        /// log to fileLog all messages of a conversation
        /// (InstantMessageReceived callback)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="fileLog"></param>
        static void remoteImModality_InstantMessageReceived(object sender, MessageSentEventArgs e, String fileLog)
        {
            InstantMessageModality modality = (InstantMessageModality)sender;

            //gets the participant name
            string name = (string)modality.Participant.Contact.GetContactInformation(ContactInformationType.DisplayName);

            //reads the message in its plain text format (automatically converted)
            string message = e.Text;

            //write message to log
            using (FileStream stream = File.Open(fileLog, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(String.Format(LOG_MESSAGE, name.Substring(name.IndexOf(", ") + 2), DateTime.Now.ToString("HH:mm:ss"), message));
                }
            }
        }

        /// <summary>
        /// Identify if a particular SystemException is one of the exceptions which may be thrown
        /// by the Lync Model API.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static bool IsLyncException(SystemException ex)
        {
            return
                ex is NotImplementedException ||
                ex is ArgumentException ||
                ex is NullReferenceException ||
                ex is NotSupportedException ||
                ex is ArgumentOutOfRangeException ||
                ex is IndexOutOfRangeException ||
                ex is InvalidOperationException ||
                ex is TypeLoadException ||
                ex is TypeInitializationException ||
                ex is InvalidComObjectException ||
                ex is InvalidCastException;
        }
    }
}
