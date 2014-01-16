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

        /// <summary>
        /// add notification icon to system tray bar (near the clock)
        /// quit option is available by default
        /// </summary>
        /// <param name="name">name displayed on mouse hover</param>
        /// <param name="items">items to add to the context menu</param>
        private static void addNotifyIcon(String name, MenuItem[] items)
        {
            var notifyIcon = new System.Windows.Forms.NotifyIcon();
            string iconPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\icon.ico"; //icon in base folder
            try
            {
                notifyIcon.Icon = new Icon(iconPath);
            }
            catch (Exception)
            { //dev mode
                notifyIcon.Icon = new Icon(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\..\..\icon.ico");
            }
            notifyIcon.Text = name;
            notifyIcon.Visible = true;

            ContextMenu contextMenu1 = new ContextMenu();
            contextMenu1.MenuItems.AddRange(items);
            contextMenu1.MenuItems.Add(new MenuItem("Quit", (s, e) => { notifyIcon.Dispose();  Process.GetCurrentProcess().Kill(); }));

            notifyIcon.ContextMenu = contextMenu1;
        }
        
        static void Main(string[] args)
        {
            // create log directory if missing
            createDirectoryIfMissing(LOG_FOLDER);

            //-- -- -- Add notification icon
            addNotifyIcon("Lync Logger", new MenuItem[] {
                new MenuItem("Lync History", (s, e) => { Process.Start(LOG_FOLDER); })
            });

            //-- -- -- Handles LYNC operations
            new LyncLogger(LOG_FOLDER);

            //prevent the application from exiting right away
            Application.Run();
        }
        
    }
}
