using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using TM_Comms_WPF;

namespace ApplicationSettingsNS
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
            public string RobotIP { get; set; } = "192.168.1.1";

            public TMflowVersions Version { get; set; } = TMflowVersions.V1_80_xxxx;

            public string[] ModbusComboBoxIndices { get; set; } = {
                "Current Base With Tool X",
                "Current Base With Tool Y",
                "Current Base With Tool Z",
                "Current Base With Tool Rx",
                "Current Base With Tool Ry",
                "Current Base With Tool Rz",
                "TCP Force X",
                "TCP Force Y",
                "TCP Force Z",
                "TCP Force 3D"};

            public string[] ModbusUserComboBoxIndices { get; set; } = {
                "Int16",
                "Int16",
                "Int16",
                "Int16",
                "Int16",
                "Int16",
                "Int16",
                "Int16",
                "Int16",
                "Int16"};

            public WindowSettings MainWindow { get; set; } = new WindowSettings();
            public WindowSettings ModbusWindow { get; set; } = new WindowSettings();
            public WindowSettings ListenNodeWindow { get; set; } = new WindowSettings();
            public WindowSettings EthernetSlaveWindow { get; set; } = new WindowSettings();
            public WindowSettings Port8080Window { get; set; } = new WindowSettings();

            public class WindowSettings
            {
                public double Left { get; set; } = 0;
                public double Top { get; set; } = 0;
                public double Width { get; set; } = 1024;
                public double Height { get; set; } = 768;
                public WindowState WindowState { get; set; } = WindowState.Normal;
            }
        }
    }
}
