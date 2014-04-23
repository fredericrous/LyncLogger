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

namespace LyncLogger
{
    class Program
    {
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
                new LyncLogger(LOG_FOLDER);
            };
            bw.RunWorkerAsync();
            
            //prevent the application from exiting right away
            Application.Run();
        }
        
    }
}
