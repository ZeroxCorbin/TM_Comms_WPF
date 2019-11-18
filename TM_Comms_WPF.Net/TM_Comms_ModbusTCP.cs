using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wisdom.Utils.Driver;
using Wisdom.Utils.Driver.Modbus;

namespace TM_Comms_WPF.Net
{
    public class TM_Comms_ModbusTCP : IDisposable
    {
        public bool Connected
        {
            get { return tcpModbus.Connected; }
        }

        public ModbusTcpDriver tcpModbus;

        public TM_Comms_ModbusTCP()
        {
            tcpModbus = new ModbusTcpDriver("Testing");

        }

        public bool Connect(string ip)
        {
            try
            {
                tcpModbus.Connect(ip);
            }
            catch
            {

            }
            
            if (tcpModbus.Connected)
                return true;
            else
                return false;
        }

        public void Disconnect()
        {
            tcpModbus.Disconnect();
        }

        

        public bool GetBool(int addr)
        {
            return tcpModbus.GetOneDiscreteBit(addr);
            //return Convert.ToBoolean(byteArray[0]);
        }

        public int GetInt16(int addr)
        {
            byte[] byteArray = tcpModbus.GetMultipleBytes(addr, 1);
            Array.Reverse(byteArray);
            return System.BitConverter.ToInt16(byteArray, 0);
        }

        public int GetInt32(int addr)
        {
            byte[] byteArray = tcpModbus.GetMultipleBytes(addr, 2);
            Array.Reverse(byteArray);
            return System.BitConverter.ToInt32(byteArray, 0);
        }

        public float GetFloat(int addr)
        {
            byte[] byteArray = tcpModbus.GetMultipleBytes(addr, 2);
            Array.Reverse(byteArray);
            return System.BitConverter.ToSingle(byteArray, 0);
        }

        public string GetString(int addr)
        {
            
            byte[] byteArray = tcpModbus.GetMultipleBytes(addr, 5);
            Array.Reverse(byteArray);
            return Encoding.UTF8.GetString(byteArray);
        }

        public void SetBool(int addr, bool val)
        {
            tcpModbus.SendOneBit(addr, val);
        }

        public void SetInt16(int addr, int val)
        {
            byte[] byteArray = BitConverter.GetBytes(val);
            Array.Reverse(byteArray);
            tcpModbus.SendMultipleWords(addr, byteArray);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    tcpModbus?.Dispose();
                }

                tcpModbus?.Disconnect();
                    
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~TM_Comms_ModbusTCP()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion


    }
}
