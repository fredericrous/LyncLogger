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
            string text = String.Format("{0}\nstatus: {1}", _name, status ? "on" : "off");

            string nameIcon = status ? "icon.ico" : "icon_off.ico";

            setNotifyIcon(nameIcon, text);
        }
        /// <summary>
        /// This delegate allows us to call LoggerStatus_DelegateMethod in the backgroundworker
        /// It changes the indicator that displays the state of the app.
        /// </summary>
        public static LoggerStatus ChangeLoggerStatus = LoggerStatus_DelegateMethod;

        /// <summary>
        /// set text and icon for the taskbar
        /// icon must be in same folder or solution folder
        /// </summary>
        /// <param name="nameIcon"></param>
        /// <param name="text"></param>
        public static void setNotifyIcon(string nameIcon, string text)
        {
            string currDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string iconPath = String.Format(@"{0}\{1}", currDirectory, nameIcon); //icon in base folder

            //set text that support 128 char instead of 64
            Fixes.SetNotifyIconText(notifyIcon, text);

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
                disposeNotifyIcon();
            }));
            notifyIcon.ContextMenu = contextMenu1;

        }

        public static void disposeNotifyIcon()
        {
            notifyIcon.Dispose();
            foreach (ProcessThread thread in Process.GetCurrentProcess().Threads)
            {
                thread.Dispose();
            }
            Process.GetCurrentProcess().Kill();
        }
    }

    public class Fixes
    {
        /// <summary>
        /// Set text tooltip to 128 char limit instead of 64
        /// http://stackoverflow.com/questions/579665/how-can-i-show-a-systray-tooltip-longer-than-63-chars
        /// </summary>
        /// <param name="ni"></param>
        /// <param name="text"></param>
        public static void SetNotifyIconText(NotifyIcon ni, string text)
        {
            if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");

            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);

            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }
    }
}
