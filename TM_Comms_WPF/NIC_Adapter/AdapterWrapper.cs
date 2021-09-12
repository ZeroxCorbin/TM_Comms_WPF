using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InterManage.Adapter
{
    public class AdapterWrapper : IDisposable
    {
        private ManagementObject _man;
        public AdapterWrapper(ManagementObject m)
        {
            _man = m;
        }

        private object GetParameter(string paramName, Type type)
        {
            object obj = _man.GetPropertyValue(paramName);
            if (obj != null) return obj;
            if (type.IsValueType) return Activator.CreateInstance(type);
            if (type.IsArray) return Activator.CreateInstance(type, new object[] { 1 });
            return null;
        }
        private UInt32 InvokeMethod(string methodName, ManagementBaseObject man, InvokeMethodOptions options)
        {
            ManagementBaseObject obj = _man.InvokeMethod(methodName, man, options);
            object obj1 = obj["ReturnValue"];
            return (UInt32)obj1;
        }

        public object this[string propertyName]
        {
            get
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                return property.GetValue(this, null);
            }
            set
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                Type propType = property.PropertyType;
                if (value == null)
                {
                    if (propType.IsValueType && Nullable.GetUnderlyingType(propType) == null)
                    {
                        throw new InvalidCastException();
                    }
                    else
                    {
                        property.SetValue(this, null, null);
                    }
                }
                else if (value.GetType() == propType)
                {
                    property.SetValue(this, value, null);
                }
                else
                {
                    TypeConverter typeConverter = TypeDescriptor.GetConverter(propType);
                    object propValue = typeConverter.ConvertFromString(value.ToString());
                    property.SetValue(this, propValue, null);
                }
            }
        }

        readonly IDictionary<UInt16, string> Availability_Dict = new Dictionary<UInt16, string>
        {
            {0,"Undefined"},
{1,"Other"},
{2,"Unknown"},
{3,"Running/Full Power"},
{4,"Warning"},
{5,"In Test"},
{6,"Not Applicable"},
{7,"Power Off"},
{8,"Off Line"},
{9,"Off Duty"},
{10,"Degraded"},
{11,"Not Installed"},
{12,"Install Error"},
{13,"Power Save - Unknown"},
{14,"Power Save - Low Power Mode"},
{15,"Power Save - Standby"},
{16,"Power Cycle"},
{17,"Power Save - Warning"},
{18,"Paused"},
{19,"Not Ready"},
{20,"Not Configured"},
{21,"Quiesced"}

        };
        readonly IDictionary<UInt16, string> NetConnectionStatus_Dict = new Dictionary<UInt16, string>
        {
            {0,"Disconnected"},
            {1,"Connecting"},
            {2,"Connected"},
            {3,"Disconnecting"},
            {4,"Hardware Not Present"},
            {5,"Hardware Disabled"},
            {6,"Hardware Malfunction"},
            {7,"Media Disconnected"},
            {8,"Authenticating"},
            {9,"Authentication Succeeded"},
            {10,"Authentication Failed"},
            {11,"Invalid Address"},
            {12,"Credentials Required"},
        };
        readonly IDictionary<UInt16, string> PowerManagementCapabilities_Dict = new Dictionary<UInt16, string>
        {
            {0,"Unknown"},
            {1,"Not Supported"},
            {2,"Disabled"},
            {3,"Enabled"},
            {4,"Power Saving Modes Entered Automatically"},
            {5,"Power State Settable"},
            {6,"Power Cycling Supported"},
            {7,"Timed Power On Supported"}
        };
        readonly IDictionary<UInt16, string> StatusInfo_Dict = new Dictionary<UInt16, string>
        {
            {0,"Undefined"},
            {1,"Other"},
            {2,"Unknown"},
            {3,"Enabled"},
            {4,"Disabled"},
            {5,"Not Applicable"}
        };

        public void Dispose()
        {
            _man.Dispose();
        }

        public string AdapterType
        {
            get { return (string)GetParameter("AdapterType", typeof(string)); }
        }
        public UInt16 AdapterTypeID
        {
            get { return (UInt16)GetParameter("AdapterTypeID", typeof(UInt16)); }
        }
        public bool AutoSense
        {
            get { return (bool)GetParameter("AutoSense", typeof(bool)); }
        }
        public UInt16 Availability
        {
            get { return (UInt16)GetParameter("Availability", typeof(UInt16)); }
        }
        public string Availability_String
        {
            get { return Availability_Dict[this.Availability]; }
        }
        public string Caption
        {
            get { return (string)GetParameter("Caption", typeof(string)); }
        }
        public UInt32 ConfigManagerErrorCode
        {
            get { return (UInt32)GetParameter("ConfigManagerErrorCode", typeof(UInt32)); }
        }
        public bool ConfigManagerUserConfig
        {
            get { return (bool)GetParameter("ConfigManagerUserConfig", typeof(bool)); }
        }
        public string CreationClassName
        {
            get { return (string)GetParameter("CreationClassName", typeof(string)); }
        }
        public string Description
        {
            get { return (string)GetParameter("Description", typeof(string)); }
        }
        public string DeviceID
        {
            get { return (string)GetParameter("DeviceID", typeof(string)); }
        }
        public bool ErrorCleared
        {
            get { return (bool)GetParameter("ErrorCleared", typeof(bool)); }
        }
        public string ErrorDescription
        {
            get { return (string)GetParameter("ErrorDescription", typeof(string)); }
        }
        public string GUID
        {
            get { return (string)GetParameter("GUID", typeof(string)); }
        }
        public UInt32 Index
        {
            get { return (UInt32)GetParameter("Index", typeof(UInt32)); }
        }
        public DateTime InstallDate
        {
            get
            {
                object obj = GetParameter("InstallDate", typeof(DateTime));
                if (obj.GetType() == typeof(DateTime)) return (DateTime)obj;
                //DMTF datetime
                if (obj.GetType() == typeof(string)) return ManagementDateTimeConverter.ToDateTime((string)obj);
                else return new DateTime();
            }
        }
        public bool Installed
        {
            get { return (bool)GetParameter("Installed", typeof(bool)); }
        }
        public UInt32 InterfaceIndex
        {
            get { return (UInt32)GetParameter("InterfaceIndex", typeof(UInt32)); }
        }
        public UInt32 LastErrorCode
        {
            get { return (UInt32)GetParameter("LastErrorCode", typeof(UInt32)); }
        }
        public string MACAddress
        {
            get { return (string)GetParameter("MACAddress", typeof(string)); }
        }
        public string Manufacturer
        {
            get { return (string)GetParameter("Manufacturer", typeof(string)); }
        }
        public UInt32 MaxNumberControlled
        {
            get { return (UInt32)GetParameter("MaxNumberControlled", typeof(UInt32)); }
        }
        public UInt64 MaxSpeed
        {
            get { return (UInt64)GetParameter("MaxSpeed", typeof(UInt64)); }
        }
        public string Name
        {
            get { return (string)GetParameter("Name", typeof(string)); }
        }
        public string NetConnectionID
        {
            get { return (string)GetParameter("NetConnectionID", typeof(string)); }
        }
        public UInt16 NetConnectionStatus
        {
            get { return (UInt16)GetParameter("NetConnectionStatus", typeof(UInt16)); }
        }
        public string NetConnectionStatus_String
        {
            get { return NetConnectionStatus_Dict[this.NetConnectionStatus]; }
        }
        public bool NetEnabled
        {
            get { return (bool)GetParameter("NetEnabled", typeof(bool)); }
        }
        public string[] NetworkAddresses
        {
            get { return (string[])GetParameter("NetworkAddresses", typeof(string[])); }
        }
        public string PermanentAddress
        {
            get { return (string)GetParameter("PermanentAddress", typeof(string)); }
        }
        public bool PhysicalAdapter
        {
            get { return (bool)GetParameter("PhysicalAdapter", typeof(bool)); }
        }
        public string PNPDeviceID
        {
            get { return (string)GetParameter("PNPDeviceID", typeof(string)); }
        }
        public UInt16[] PowerManagementCapabilities
        {
            get { return (UInt16[])GetParameter("PowerManagementCapabilities", typeof(UInt16[])); }
        }
        public string PowerManagementCapabilities_String
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach(UInt16 val in this.PowerManagementCapabilities) sb.AppendLine(PowerManagementCapabilities_Dict[val]);
                return sb.ToString();
            }
        }
        public bool PowerManagementSupported
        {
            get { return (bool)GetParameter("PowerManagementSupported", typeof(bool)); }
        }
        public string ProductName
        {
            get { return (string)GetParameter("ProductName", typeof(string)); }
        }
        public string ServiceName
        {
            get { return (string)GetParameter("ServiceName", typeof(string)); }
        }
        public UInt64 Speed
        {
            get { return (UInt64)GetParameter("Speed", typeof(UInt64)); }
        }
        public string Status
        {
            get { return (string)GetParameter("Status", typeof(string)); }
        }
        public UInt16 StatusInfo
        {
            get { return (UInt16)GetParameter("StatusInfo", typeof(UInt16)); }
        }
        public string StatusInfo_String
        {
            get { return StatusInfo_Dict[this.StatusInfo]; }
        }
        public string SystemCreationClassName
        {
            get { return (string)GetParameter("SystemCreationClassName", typeof(string)); }
        }
        public string SystemName
        {
            get { return (string)GetParameter("SystemName", typeof(string)); }
        }
        public DateTime TimeOfLastReset
        {
            get
            {
                object obj = GetParameter("TimeOfLastReset", typeof(DateTime));
                if (obj.GetType() == typeof(DateTime)) return (DateTime)obj;
                //DMTF datetime
                if (obj.GetType() == typeof(string)) return ManagementDateTimeConverter.ToDateTime((string)obj);
                else return new DateTime();
            }
        }

        public UInt32 Disable() //Disables the network adapter.
        {
            return InvokeMethod("Disable", null, null);
        }
        public UInt32 Enable() //Enables the network adapter.
        {
            return InvokeMethod("Enable", null, null);
        }
    }
}
