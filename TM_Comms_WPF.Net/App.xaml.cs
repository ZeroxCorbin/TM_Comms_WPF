using ApplicationSettingsNS;
using System;
using System.Windows;

namespace TM_Comms_WPF.Net
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationSettings_Serializer.ApplicationSettings Settings;

        public App() => Settings = ApplicationSettings_Serializer.Load("appsettings.xml");

        protected override void OnStartup(StartupEventArgs e) => base.OnStartup(e);
    }
}
