﻿using System;
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
                new MenuItem("Lync History", (s, e) => { Process.Start(LOG_FOLDER); })
            });

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
                    string error_msg = "Software is missing dlls, please visit https://github.com/Zougi/LyncLogger to read how to install the requirements";
                    _log.Error(error_msg);

                    //set a user friendly error
                    NotifyIconSystray.setNotifyIcon("icon_ooo.ico", error_msg);
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
        
    }
}