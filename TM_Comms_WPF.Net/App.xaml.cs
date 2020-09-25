using ApplicationSettingsNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace TM_Comms_WPF
{

    public class MoveToForeground
    {
        [DllImportAttribute("User32.dll")]
        private static extern int FindWindow(String ClassName, String WindowName);

        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_SHOWWINDOW = 0x0040;
        const int SWP_NOACTIVATE = 0x0010;
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        public static void DoOnProcess(string processName)
        {
            var allProcs = Process.GetProcessesByName(processName);
            if (allProcs.Length > 0)
            {
                Process proc = allProcs[0];
                int hWnd = FindWindow(null, proc.MainWindowTitle.ToString());
                // Change behavior by settings the wFlags params. See http://msdn.microsoft.com/en-us/library/ms633545(VS.85).aspx
                SetWindowPos(new IntPtr(hWnd), 0, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }
        }
    }

    public class CheckOnScreen
    {

        public static bool IsOnScreen(Window window)
        {
            System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;
            foreach (System.Windows.Forms.Screen screen in screens)
            {
                System.Drawing.Rectangle formRectangle = new System.Drawing.Rectangle((int)window.Left * 2, (int)window.Top * 2,
                                                         (int)window.Width * 2, (int)window.Height * 2);

                if (screen.WorkingArea.Contains(formRectangle))
                {
                    return true;
                }
            }

            return false;
        }


    }


    public partial class App : Application
    {
        public class GetData
        {

            public List<string> Commands { get; private set; } = new List<string>();
            public GetData()
            {
                using (StreamReader file = new StreamReader("ListenNodeCommands_Raw_1.68.6800.txt"))
                {
                    StringBuilder sb = new StringBuilder();

                    bool build = false;
                    string ln;
                    while ((ln = file.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(ln, @"^[0-9][.][0-9]\w*"))
                        {
                            Commands.Add(ln.Trim(new char[] { '\r', '\n' }));
                            continue;
                        }

                        if (build)
                        {
                            if (ln.StartsWith("Ex"))
                                continue;

                            if (ln.StartsWith("Con"))
                                continue;

                            sb.Append(ln.Trim(new char[] { '\r', '\n' }));

                            if (ln.Contains(")"))
                            {
                                build = false;
                                Commands.Add(sb.ToString());
                                sb.Clear();
                            }
                        }

                        if (ln.StartsWith("syntax", StringComparison.OrdinalIgnoreCase))
                            build = true;
                    }
                }
                File.WriteAllLines("ListenNodeCommands.txt", Commands.ToArray());
            }
        }

        public static ApplicationSettings_Serializer.ApplicationSettings Settings { get; set; }
#if DEBUG
        public static string SettingsFileRootDir { get; set; } = System.IO.Directory.GetCurrentDirectory();
#else        
        public static string SettingsFileRootDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
        public static string SettingsFileAppDir { get; set; } = "\\Nexus\\TM_Comms_WPF\\";
        public static string SettingsFileName { get; set; } = "appsettings.xml";
        public static string SettingsFilePath { get; set; } = SettingsFileRootDir + SettingsFileAppDir + SettingsFileName;

        public static string Path { get; set; } = System.IO.Directory.GetCurrentDirectory();
        public App()
        {
            if (!Directory.Exists(SettingsFileRootDir + SettingsFileAppDir))
            {
                try
                {
                    Directory.CreateDirectory(SettingsFileRootDir + SettingsFileAppDir);
                }
                catch (Exception)
                {
                }
            }
            try
            {
                Settings = ApplicationSettings_Serializer.Load(SettingsFilePath);
            }
            catch (Exception)
            {
                Settings = new ApplicationSettings_Serializer.ApplicationSettings();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //GetData d = new GetData();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                ApplicationSettings_Serializer.Save(SettingsFilePath, Settings);
            }
            catch (Exception)
            {
            }

            base.OnExit(e);
        }
    }
}
