using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;

namespace InstallerActions
{
    [RunInstaller(true)]
    public partial class RemoveFiles : Installer
    {
        public static string SettingsFileRootDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string SettingsFileAppDir { get; set; } = "\\Nexus\\TM_Comms_WPF\\";
        public static string SettingsFileName { get; set; } = "appsettings.xml";

        protected override void OnAfterUninstall(IDictionary savedState)
        {
            System.IO.File.Delete(SettingsFileName);
            System.IO.Directory.Delete(SettingsFileRootDir + SettingsFileAppDir, true);
        }
    }
}
