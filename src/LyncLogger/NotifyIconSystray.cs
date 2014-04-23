using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyncLogger
{
    /// <summary>
    /// Handles the systray icon
    /// </summary>
    static class NotifyIconSystray
    {
        private static NotifyIcon notifyIcon;
        public delegate void LoggerStatus(bool status);
        private static string _name;

        /// <summary>
        /// This method allows to change the state of icon and tooltip
        /// true = Log Active: the logger detected the client and is active.
        /// </summary>
        /// <param name="status"></param>
        public static void LoggerStatus_DelegateMethod(bool status)
        {
            notifyIcon.Text = String.Format("{0}\nstatus: {1}", _name, status ? "on" : "off");

            string nameIcon = status ? "icon.ico" : "icon_off.ico";

            string currDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string iconPath = String.Format(@"{0}\{1}", currDirectory, nameIcon); //icon in base folder
            try
            {
                notifyIcon.Icon = new Icon(iconPath);
            }
            catch (Exception)
            { //dev mode
                notifyIcon.Icon = new Icon(String.Format(@"{0}\..\..\{1}", currDirectory, nameIcon));
            }
        }
        /// <summary>
        /// This delegate allows us to call LoggerStatus_DelegateMethod in the backgroundworker
        /// It changes the indicator that displays the state of the app.
        /// </summary>
        public static LoggerStatus ChangeLoggerStatus = LoggerStatus_DelegateMethod;

        /// <summary>
        /// add notification icon to system tray bar (near the clock)
        /// quit option is available by default
        /// </summary>
        /// <param name="name">name displayed on mouse hover</param>
        /// <param name="items">items to add to the context menu</param>
        public static void addNotifyIcon(String name, MenuItem[] items)
        {
            _name = name;
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Visible = true;

            LoggerStatus_DelegateMethod(false); //set name and icon

            ContextMenu contextMenu1 = new ContextMenu();
            contextMenu1.MenuItems.AddRange(items);
            contextMenu1.MenuItems.Add(new MenuItem("Quit", (s, e) =>
            {
                notifyIcon.Dispose();
                foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
	            {
                    thread.Dispose();
	            }
                Process.GetCurrentProcess().Kill();
            }));
            notifyIcon.ContextMenu = contextMenu1;

        }
    }
}
