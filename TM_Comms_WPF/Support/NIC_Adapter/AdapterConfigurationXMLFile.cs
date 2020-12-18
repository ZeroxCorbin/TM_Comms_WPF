using System;
using System.IO;
using System.Xml.Serialization;

namespace InterManage.Adapter
{
    public class AdapterConfigurationXMLFile
    {
        public static AdapterConfigurationXMLFile.XMLRoot Load(string file)
        {
            StreamReader sr;
            AdapterConfigurationXMLFile.XMLRoot app;
            XmlSerializer serializer = new XmlSerializer(typeof(AdapterConfigurationXMLFile.XMLRoot));
            try
            {
                sr = new StreamReader(file);
            }
            catch (FileNotFoundException)
            {
                AdapterConfigurationXMLFile.Save(file, new AdapterConfigurationXMLFile.XMLRoot());
                sr = new StreamReader(file);
            }

            app = (AdapterConfigurationXMLFile.XMLRoot)serializer.Deserialize(sr);
            sr.Close();
            return app;
        }

        public static void Save(string file, XMLRoot app)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(XMLRoot));
            using (StreamWriter sw = new StreamWriter(file))
            {
                serializer.Serialize(sw, app);
            }
        }

        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "InterManage.Adapter")]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "InterManage.Adapter", IsNullable = false)]
        public partial class XMLRoot
        {
            public XMLRoot() { }
            public XMLRoot(bool init = true)
            {
                if (!init) return;

                DefaultIPGateway = new string[0];
                DNSDomainSuffixSearchOrder = new string[0]; ;
                DNSServerSearchOrder = new string[0]; ;
                IPAddress = new string[0]; ;
                IPSecPermitIPProtocols = new string[0]; ;
                IPSecPermitTCPPorts = new string[0]; ;
                IPSecPermitUDPPorts = new string[0]; ;
                IPSubnet = new string[0]; ;
                IPXNetworkNumber = new string[0]; ;

                GatewayCostMetric = new UInt16[0];
                IPXFrameType = new UInt32[0];
            }

            public string Caption
            {
                get; set;
            }
            public string Description
            {
                get; set;
            }
            public string SettingID
            {
                get; set;
            }
            public Boolean ArpAlwaysSourceRoute
            {
                get; set;
            }
            public Boolean ArpUseEtherSNAP
            {
                get; set;
            }
            public string DatabasePath
            {
                get; set;
            }
            public Boolean DeadGWDetectEnabled
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] DefaultIPGateway
            {
                get; set;
            }
            public uint DefaultTOS
            {
                get; set;
            }
            public uint DefaultTTL
            {
                get; set;
            }
            public Boolean DHCPEnabled
            {
                get; set;
            }
            public DateTime DHCPLeaseExpires
            {
                get; set;
            }
            public DateTime DHCPLeaseObtained
            {
                get; set;
            }
            public string DHCPServer
            {
                get; set;
            }
            public string DNSDomain
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] DNSDomainSuffixSearchOrder
            {
                get; set;
            }
            public Boolean DNSEnabledForWINSResolution
            {
                get; set;
            }
            public string DNSHostName
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] DNSServerSearchOrder
            {
                get; set;
            }
            public Boolean DomainDNSRegistrationEnabled
            {
                get; set;
            }
            public UInt32 ForwardBufferMemory
            {
                get; set;
            }
            public Boolean FullDNSRegistrationEnabled
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public UInt16[] GatewayCostMetric
            {
                get; set;
            }
            public uint IGMPLevel
            {
                get; set;
            }
            public UInt32 Index
            {
                get; set;
            }
            public UInt32 InterfaceIndex
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] IPAddress
            {
                get; set;
            }
            public UInt32 IPConnectionMetric
            {
                get; set;
            }
            public Boolean IPEnabled
            {
                get; set;
            }
            public Boolean IPFilterSecurityEnabled
            {
                get; set;
            }
            public Boolean IPPortSecurityEnabled
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] IPSecPermitIPProtocols
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] IPSecPermitTCPPorts
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] IPSecPermitUDPPorts
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] IPSubnet
            {
                get; set;
            }
            public Boolean IPUseZeroBroadcast
            {
                get; set;
            }
            public string IPXAddress
            {
                get; set;
            }
            public Boolean IPXEnabled
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public UInt32[] IPXFrameType
            {
                get; set;
            }
            public UInt32 IPXMediaType
            {
                get; set;
            }
            [System.Xml.Serialization.XmlArrayItem(IsNullable = false)]
            public string[] IPXNetworkNumber
            {
                get; set;
            }
            public string IPXVirtualNetNumber
            {
                get; set;
            }
            public UInt32 KeepAliveInterval
            {
                get; set;
            }
            public UInt32 KeepAliveTime
            {
                get; set;
            }
            public string MACAddress
            {
                get; set;
            }
            public UInt32 MTU
            {
                get; set;
            }
            public UInt32 NumForwardPackets
            {
                get; set;
            }
            public Boolean PMTUBHDetectEnabled
            {
                get; set;
            }
            public Boolean PMTUDiscoveryEnabled
            {
                get; set;
            }
            public string ServiceName
            {
                get; set;
            }
            public UInt32 TcpipNetbiosOptions
            {
                get; set;
            }
            public UInt32 TcpMaxConnectRetransmissions
            {
                get; set;
            }
            public UInt32 TcpMaxDataRetransmissions
            {
                get; set;
            }
            public UInt32 TcpNumConnections
            {
                get; set;
            }
            public Boolean TcpUseRFC1122UrgentPointer
            {
                get; set;
            }
            public UInt16 TcpWindowSize
            {
                get; set;
            }
            public Boolean WINSEnableLMHostsLookup
            {
                get; set;
            }
            public string WINSHostLookupFile
            {
                get; set;
            }
            public string WINSPrimaryServer
            {
                get; set;
            }
            public string WINSScopeID
            {
                get; set;
            }
            public string WINSSecondaryServer
            {
                get; set;
            }
        }
    }
}
