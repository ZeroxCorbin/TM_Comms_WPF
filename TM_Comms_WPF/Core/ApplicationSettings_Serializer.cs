using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Transactions;
using System.Windows;
using System.Xml.Serialization;
using TM_Comms;

namespace TM_Comms_WPF.Core
{
    public class ApplicationSettings_Serializer
    {
        public static ApplicationSettings Load(string file)
        {
            StreamReader sr;
            ApplicationSettings app;
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
            try
            {
                sr = new StreamReader(file);
            }
            catch (FileNotFoundException)
            {
                ApplicationSettings_Serializer.Save(file, new ApplicationSettings());
                sr = new StreamReader(file);
            }

            app = (ApplicationSettings)serializer.Deserialize(sr);
            sr.Close();
            return app;
        }
        public static void Save(string file, ApplicationSettings app)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
            using (StreamWriter sw = new StreamWriter(file))
            {
                serializer.Serialize(sw, app);
            }
        }

        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class ApplicationSettings
        {
            public string Theme { get; set; } = "Light.Steel";

            public string RobotIP { get; set; } = "";

            public TMflowVersions Version { get; set; } = TMflowVersions.V1_84_xxxx;

            public string ExternalVisionWindow_ListenAddress { get; set; } = "*";
            public int ExternalVisionWindow_ListenPort { get; set; } = 8080;

            public WindowSettings MainWindow { get; set; } = new WindowSettings();
            public WindowSettings Port8080Window { get; set; } = new WindowSettings();

            public class WindowSettings : INotifyPropertyChanged
            {
                private double _left = 0;
                private double _top = 0;
                private double _width = 1024;
                private double _height = 768;
                private WindowState windowState = WindowState.Normal;

                public double Left
                {
                    get { return _left; }
                    set { if(windowState == WindowState.Normal) _left = value; RaisePropertyChanged("Left"); }
                }
                public double Top
                {
                    get { return _top; }
                    set { if(windowState == WindowState.Normal) _top = value; RaisePropertyChanged("Top"); }
                }
                public double Width
                {
                    get { return _width; }
                    set { if(windowState == WindowState.Normal) _width = value; RaisePropertyChanged("Width"); }
                }
                public double Height
                {
                    get { return _height; }
                    set { if(windowState == WindowState.Normal) _height = value; RaisePropertyChanged("Height"); }
                }
                public WindowState WindowState
                {
                    get { return windowState; }
                    set { if(value != WindowState.Minimized) windowState = value; RaisePropertyChanged("WindowState"); }
                }

                public event PropertyChangedEventHandler PropertyChanged;
                private void RaisePropertyChanged(string propertyName) =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
