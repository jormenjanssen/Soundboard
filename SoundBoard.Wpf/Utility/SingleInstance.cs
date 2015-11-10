using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SoundBoard.Wpf.Utility
{
    /* All of the code below can optionally be put in a class library and reused with all your applications. */

    /*
    *	SingeInstance
    *
    *	This is where the magic happens.
    *
    *	Start() tries to create a mutex.
    *	If it detects that another instance is already using the mutex, then it returns FALSE.
    *	Otherwise it returns TRUE.
    *	(Notice that a GUID is used for the mutex name, which is a little better than using the application name.)
    *
    *	If another instance is detected, then you can use ShowFirstInstance() to show it
    *	(which will work as long as you override WndProc as shown above).
    *
    *	ShowFirstInstance() broadcasts a message to all windows.
    *	The message is WM_SHOWFIRSTINSTANCE.
    *	(Notice that a GUID is used for WM_SHOWFIRSTINSTANCE.
    *	That allows you to reuse this code in multiple applications without getting
    *	strange results when you run them all at the same time.)
    *
    */

    // using System.Threading;

    public static class SingleInstance
    {
        #region Private fields

        private static Mutex _mutex;

        #endregion

        #region Public methods

        public static void ShowFirstInstance()
        {
            WinApi.PostMessage(
                (IntPtr)WinApi.HWND_BROADCAST,
                WM_SHOWFIRSTINSTANCE,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        public static bool Start()
        {
            var onlyInstance = false;
            var mutexName = string.Format("Local\\{0}", ProgramInfo.AssemblyGuid);

            // if you want your app to be limited to a single instance
            // across ALL SESSIONS (multiple users & terminal services), then use the following line instead:
            // string mutexName = String.Format("Global\\{0}", ProgramInfo.AssemblyGuid);

            _mutex = new Mutex(true, mutexName, out onlyInstance);
            return onlyInstance;
        }

        public static void Stop()
        {
            _mutex.ReleaseMutex();
        }

        #endregion

        public static readonly int WM_SHOWFIRSTINSTANCE =
            WinApi.RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", ProgramInfo.AssemblyGuid);
    }

    /*
    *	WinApi
    *
    *	This class is just a wrapper for your various WinApi functions.
    *
    *	In this sample only the bare essentials are included.
    *	In my own WinApi class, I have all the WinApi functions that any
    *	of my applications would ever need.
    *
    */

    // using System.Runtime.InteropServices;

    public static class WinApi
    {
        #region Public methods

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        public static int RegisterWindowMessage(string format, params object[] args)
        {
            var message = String.Format(format, args);
            return RegisterWindowMessage(message);
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowToFront(IntPtr window)
        {
            ShowWindow(window, SW_SHOWNORMAL);
            SetForegroundWindow(window);
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion

        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;
    }

    /*
    *	ProgramInfo
    *
    *	This class is just for getting information about the application.
    *	Each assembly has a GUID, and that GUID is useful to us in this application,
    *	so the most important thing in this class is the AssemblyGuid property.
    *
    *	GetEntryAssembly() is used instead of GetExecutingAssembly(), so that you
    *	can put this code into a class library and still get the results you expect.
    *	(Otherwise it would return info on the DLL assembly instead of your application.)
    */

    // using System.Reflection;

    public static class ProgramInfo
    {
        #region Public properties

        public static string AssemblyGuid
        {
            get
            {
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
                if (attributes.Length == 0)
                {
                    return string.Empty;
                }
                return ((GuidAttribute)attributes[0]).Value;
            }
        }

        public static string AssemblyTitle
        {
            get
            {
                var attributes = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    var titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().CodeBase);
            }
        }

        #endregion
    }
}