using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace InterManage.Adapter
{
    public class AdapterConfigurationManager
    {
        public List<AdapterConfigurationWrapper> GetAdapterList()
        {
            List<AdapterConfigurationWrapper> adp = new List<AdapterConfigurationWrapper>();
            using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (ManagementObject adapter in networkConfigs)
                    {
                        adp.Add(new AdapterConfigurationWrapper(adapter));
                    }
                }
            }
            return adp;
        }

        public AdapterConfigurationWrapper GetAdapter(uint interfaceIndex)
        {
            using (ManagementObjectCollection moc = new ManagementObjectSearcher(new SelectQuery("Win32_NetworkAdapterConfiguration", "InterfaceIndex=" + interfaceIndex.ToString())).Get())
            {
                if (moc.Count == 1)
                {
                    return new AdapterConfigurationWrapper(moc.Cast<ManagementObject>().First());
                }
            }
            return null;
        }

        public void SetStatic(string[] ipAddress, string[] subnetMask, string[] defaultIPGateway, string[] dnsServerSearchOrder, uint interfaceIndex, UInt16[] gatewayCostMetric = null)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                
                if (adapter != null)
                {
                    uint u = adapter.EnableStatic(ipAddress, subnetMask);
                    if (gatewayCostMetric == null) gatewayCostMetric = new UInt16[] { 1 };
                    u = adapter.SetGateways(defaultIPGateway, gatewayCostMetric);
                    u = adapter.SetDNSServerSearchOrder(dnsServerSearchOrder);
                } 
            }
        }

        public void SetStatic(string[] ipAddress, string[] subnetMask, string[] defaultIPGateway, uint interfaceIndex, UInt16[] gatewayCostMetric = null)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.EnableStatic(ipAddress, subnetMask);
                    if (gatewayCostMetric == null) gatewayCostMetric = new UInt16[] { 1 };
                    adapter.SetGateways(defaultIPGateway, gatewayCostMetric);
                } 
            }
        }

        public void SetStatic(string[] ipAddress, string[] subnetMask, uint interfaceIndex)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.EnableStatic(ipAddress, subnetMask);
                }
            }
        }

        public void SetDNS(string[] dnsServerSearchOrder, uint interfaceIndex)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.SetDNSServerSearchOrder(dnsServerSearchOrder);
                }
            }
        }

        public void EnableDHCP(uint interfaceIndex)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.EnableDHCP();
                    adapter.SetDNSServerSearchOrder(new string[0]);
                }
            }
        }

        public void RenewDHCPLease(uint interfaceIndex)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.RenewDHCPLease();
                }
            }
        }

        public void ReleaseDHCPLease(uint interfaceIndex)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.ReleaseDHCPLease();
                }
            }
        }

        public AdapterConfigurationXMLFile.XMLRoot GetCurrentValues_XMLData(uint interfaceIndex)
        {
            AdapterConfigurationXMLFile.XMLRoot xml = new AdapterConfigurationXMLFile.XMLRoot();
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    foreach (PropertyInfo obj in typeof(AdapterConfigurationXMLFile.XMLRoot).GetProperties())
                    {
                        typeof(AdapterConfigurationXMLFile.XMLRoot).GetProperty(obj.Name).SetValue(xml, typeof(AdapterConfigurationWrapper).GetProperty(obj.Name).GetValue(adapter));
                    }

                    bool invalid = false;
                    for (int i = xml.GatewayCostMetric.Count() - 1; i >= 0; i--)
                    {
                        if (invalid) xml.GatewayCostMetric[i]++;
                        if (xml.GatewayCostMetric[i] == 0)
                        {
                            xml.GatewayCostMetric[i]++;
                            invalid = true;
                        }
                    }
                }
                return xml;
            }
        }

        public uint SetCurrentValues_XMLData(AdapterConfigurationXMLFile.XMLRoot adapterXML)
        {
            using (AdapterConfigurationWrapper adapter = this.GetAdapter(adapterXML.InterfaceIndex))
            {
                if (adapter != null)
                {
                    if (!adapterXML.DHCPEnabled)
                    {
                        uint ret;
                        if ((ret = adapter.EnableStatic(adapterXML.IPAddress, adapterXML.IPSubnet)) != 0) return ret;

                        uint i = adapter.SetGateways(adapterXML.DefaultIPGateway, adapterXML.GatewayCostMetric);
                        adapter.SetDNSServerSearchOrder(adapterXML.DNSServerSearchOrder);
                    }
                    else { adapter.EnableDHCP(); }
                }
                return 0;
            }
        }
    }
}
