using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using log4net;
using Microsoft.Win32;

namespace LyncLogger
{
    class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //folder to log conversations
        private static String LOG_FOLDER = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Lync logs");
  
        /// <summary>
        /// create directory if doesnt exist
        /// </summary>
        /// <param name="folder"></param>
        static void createDirectoryIfMissing(String folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        static void Main(string[] args)
        {
            // create log directory if missing
            createDirectoryIfMissing(LOG_FOLDER);

            //-- -- -- Add notification icon
            NotifyIconSystray.addNotifyIcon("Lync Logger", new MenuItem[] {
                new MenuItem("Lync History", (s, e) => { Process.Start(LOG_FOLDER); }),
                new MenuItem("Switch Audio logger On/Off", (s, e) => {  AudioLogger.Instance.Switch(); })
            });

            //-- -- -- Handles Sound record operations

            registerKey("Software\\LyncLogger", "Audio", "Activated");
            AudioLogger.Instance.Initialize(LOG_FOLDER);

            //-- -- -- Handles LYNC operations
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (s, e) =>
            {
                try
                {
                    new LyncLogger(LOG_FOLDER);
                }
                catch (FileNotFoundException)
                {
                    string error_msg = "Software is missing dlls, please visit https://github.com/Zougi/LyncLogger";
                    _log.Error(error_msg);

                    //set a user friendly error
                    string iconName = "icon_ooo.ico";
                    NotifyIconSystray.setNotifyIcon(iconName, error_msg);
                }
                catch (Exception ex)
                {
                    _log.Error("Lync Logger Exception", ex);
                    
                    //exit app properly
                    NotifyIconSystray.disposeNotifyIcon();
                }
            };
            bw.RunWorkerAsync();

            //prevent the application from exiting right away
            Application.Run();
        }


        /// <summary>
        /// Create registry key to keep settings of recording for audio
        /// If registry key already exists, set AudioLogger
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="valueName"></param>
        /// <param name="value"></param>
        private static void registerKey(string keyName, string valueName, string value)
        {
            RegistryKey key = Registry.CurrentUser;
            RegistryKey LyncLoggerKey = key.OpenSubKey(keyName);
            if (LyncLoggerKey != null)
            {
                AudioLogger.Instance.isAllowedRecording = ((string)LyncLoggerKey.GetValue(valueName) == value);
                LyncLoggerKey.Close();
            }
            else
            {
                RegistryKey subkey = key.CreateSubKey(keyName);
                subkey.SetValue(valueName, value);
                subkey.Close();
            }
        }
        
    }
}
