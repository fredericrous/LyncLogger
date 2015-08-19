using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyncLogger.SoundManager;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using log4net;
using System.Reflection;

namespace LyncLogger
{
    class AudioLogger
    {
        static string TEMP_FOLDER = Environment.ExpandEnvironmentVariables("%temp%\\lyncloggeraudio");
        static string WAVE_FILENAME = TEMP_FOLDER + "\\captureSpeakers.wav";
        static string MIC_FILENAME = TEMP_FOLDER + "\\captureMic.wav";
        static string _folderLog = "";
        static string _fileLog = "";

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        SoundRecorder soundRecorder;
        bool _isAllowedRecording = true;

        public bool isAllowedRecording
        {
            get { return _isAllowedRecording; }
            set { _isAllowedRecording = value; }
        }

        private static AudioLogger instance;

        private AudioLogger() { }

        public static AudioLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AudioLogger();
                }
                return instance;
            }
        }

        internal void Initialize(string folderLog)
        {
            _folderLog = folderLog;

            soundRecorder = new SoundRecorder();
        }

        /// <summary>
        /// start recording
        /// </summary>
        /// <param name="status"></param>
        public void Start(string fileLog)
        {
            if (!_isAllowedRecording)
                return;
            _fileLog = fileLog;

            if (!Directory.Exists(TEMP_FOLDER))
                Directory.CreateDirectory(TEMP_FOLDER);

            try
            {
                soundRecorder.CaptureSpeakersToWave(WAVE_FILENAME, true);

                soundRecorder.CaptureMicToWave(MIC_FILENAME);
            }
            catch (Exception ex)
            {
                _log.Error("error starting audio recording", ex);
            }
        }

        /// <summary>
        /// stop audio recording
        /// </summary>
        /// <param name="status"></param>
        public void Stop()
        {
            if (!_isAllowedRecording)
                return;

            try
            {
                soundRecorder.UnCaptureSpeakersToWave();
                soundRecorder.UnCaptureMicToWave();
            }
            catch (Exception ex)
            {
                _log.Error("error canceling audio recording", ex);
            }

            try
            {
                soundRecorder.MixerWave(TEMP_FOLDER, string.Format("{0}\\{1}", _folderLog, _fileLog));
            }
            catch (Exception ex)
            {
                _log.Error("error building audio record file", ex);
            }

            try
            {
                Directory.Delete(TEMP_FOLDER, true);
            }
            catch (Exception ex)
            {
                _log.Error("error deleting record temp folder", ex);
            }
        }

        /// <summary>
        /// Activate or Deactivate audio recording
        /// </summary>
        internal void Switch()
        {
            _isAllowedRecording = !_isAllowedRecording;
            string status = (_isAllowedRecording ? "Activated" : "Deactivated");

            RegistryKey LyncLoggerKey = Registry.CurrentUser.OpenSubKey("LyncLogger");
            if (LyncLoggerKey != null)
            {
                LyncLoggerKey.SetValue("Audio", status);
                LyncLoggerKey.Close();
            }

            MessageBox.Show("Audio logger is " + status);
        }
    }
}