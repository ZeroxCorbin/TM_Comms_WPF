using System;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices; // For PInvoke
using System.Windows.Forms; // For MessageBox

namespace InterManage.Adapter
{
    public class CNotifyAddrChange
    {
        public event EventHandler<EventArgs> AddrChangedEvent = null;

        protected Thread TheThread { get; set; }

        protected enum EEvents
        {
            AddrChange,
            Stop,
            Count,
        }
        protected AutoResetEvent[] m_aoEvents = new AutoResetEvent[(int)EEvents.Count]
        {
            new AutoResetEvent(false),	// AddrChangeEvent
			new AutoResetEvent(false),  // StopEvent
        };

        [DllImport("Iphlpapi.dll", SetLastError = true)]
        public static extern UInt32 NotifyAddrChange(ref IntPtr Handle, ref NativeOverlapped overlapped);

        public CNotifyAddrChange()
        {
            TheThread = null;
            Start();
        }

        ~CNotifyAddrChange()
        {
            Stop();
        }

        protected AutoResetEvent AddrChangeEvent
        {
            get
            {
                return m_aoEvents[(int)EEvents.AddrChange];
            }
        }

        protected AutoResetEvent StopEvent
        {
            get
            {
                return m_aoEvents[(int)EEvents.Stop];
            }
        }

        public void Start()
        {
            Stop();

            TheThread = new Thread(new ThreadStart(ThreadProc));

            TheThread.Name = "NotifyAddrChange";
            TheThread.Start();
        }

        public void Stop()
        {
            if ((TheThread != null) && TheThread.IsAlive)
            {
                StopEvent.Set();

                TheThread.Join();
            }
        }

        public void ThreadProc()
        {
            try
            {
                NativeOverlapped oOverlapped = new NativeOverlapped();
                IntPtr pnHandle = IntPtr.Zero;
                oOverlapped.EventHandle = AddrChangeEvent.SafeWaitHandle.DangerousGetHandle();

                while (true)
                {
                    UInt32 nRetVal = NotifyAddrChange(ref pnHandle, ref oOverlapped);
                    if (nRetVal == 997) // 997 == ERROR_IO_PENDING, means that it will notify us when an IP Address changes
                    {
                        // Wait for any of the events to fire
                        if (WaitHandle.WaitAny(m_aoEvents) == (Int32)EEvents.AddrChange)
                        {
                            EventHandler<EventArgs> oHandler = AddrChangedEvent;
                            if (oHandler != null)
                            {
                                oHandler(this, null);
                            }
                        }
                        else
                        {
                            // The abort event was triggered, so we exit
                            break;
                        }
                    }
                    else
                    {
                        // Handle the error
                        MessageBox.Show("Failed to register for notification. " + nRetVal.ToString());
                        break;
                    }
                }
            }
            catch (System.Exception oException)
            {
                // Handle the Error
                MessageBox.Show(oException.ToString());
            }
        }
    }

    public class AdapterManager
    {
        protected CNotifyAddrChange m_oNotifyAddrChange = null;

        public void OnAddrChangedEvent(object sender, EventArgs e)
        {
            // An IP Address has changed, so do something
        }

        public AdapterManager()
        {
        }

        ~AdapterManager()
        {
            StopEvents();
        }

        public void StartEvents()
        {
            m_oNotifyAddrChange = new CNotifyAddrChange();
            m_oNotifyAddrChange.AddrChangedEvent += OnAddrChangedEvent;
        }

        public void StopEvents()
        {
            if (m_oNotifyAddrChange != null)
            {
                m_oNotifyAddrChange.AddrChangedEvent -= OnAddrChangedEvent;
                m_oNotifyAddrChange.Stop();
            }
        }

        public List<AdapterWrapper> GetFilteredAdapterList(bool isPhysical, bool isNetEnabled)
        {
            List<AdapterWrapper> adp = new List<AdapterWrapper>();

            using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapter"))
            {
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (ManagementObject adapter in networkConfigs)
                    {
                        using (AdapterWrapper adap = new AdapterWrapper(adapter))
                        {
                            if (isPhysical)
                            {
                                if (!adap.PhysicalAdapter) continue;
                            }
                            if (isNetEnabled)
                            {
                                if (!adap.NetEnabled) continue;
                            }

                            adp.Add(adap);
                        }

                    }
                }
            }
            return adp;
        }

        public List<AdapterWrapper> GetAdapterList()
        {
            List<AdapterWrapper> adp = new List<AdapterWrapper>();
            using (var networkConfigMng = new ManagementClass("Win32_NetworkAdapter"))
            {
                using (var networkConfigs = networkConfigMng.GetInstances())
                {
                    foreach (ManagementObject adapter in networkConfigs)
                    {
                        adp.Add(new AdapterWrapper(adapter));
                    }
                }
            }
            return adp;
        }

        public AdapterWrapper GetAdapter(uint interfaceIndex)
        {
            using (ManagementObjectCollection moc = new ManagementObjectSearcher(new SelectQuery("Win32_NetworkAdapter", "InterfaceIndex=" + interfaceIndex.ToString())).Get())
            {
                if (moc.Count == 1)
                {
                    using(AdapterWrapper net = new AdapterWrapper(moc.Cast<ManagementObject>().First()))
                    {
                        return net;
                    }
                }
            }
            return null;
        }

        public void DisableAdapter(uint interfaceIndex)
        {
            using (AdapterWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.Disable();
                }
            }
        }
        public void EnableAdapter(uint interfaceIndex)
        {
            using (AdapterWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    adapter.Enable();
                }
            }
        }
        public AdapterXMLFile.XMLRoot GetCurrentValues_XMLData(uint interfaceIndex)
        {
            AdapterXMLFile.XMLRoot xml = new AdapterXMLFile.XMLRoot();
            using (AdapterWrapper adapter = this.GetAdapter(interfaceIndex))
            {
                if (adapter != null)
                {
                    foreach (PropertyInfo obj in typeof(AdapterXMLFile.XMLRoot).GetProperties())
                    {
                        typeof(AdapterXMLFile.XMLRoot).GetProperty(obj.Name).SetValue(xml, typeof(AdapterWrapper).GetProperty(obj.Name).GetValue(adapter));
                    }
                }
                return xml;
            }

        }
    }
}
