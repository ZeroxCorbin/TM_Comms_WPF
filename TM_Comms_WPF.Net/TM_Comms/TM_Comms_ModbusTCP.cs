using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TM_Comms_WPF.Net
{
    public class TM_Comms_ModbusTCP : IDisposable
    {
        public bool IsError { get; private set; } = false;

        public bool Connected => Socket.IsConnected;

        public SocketManagerNS.SocketManager Socket;

        public bool Connect(string ip, int port = 502)
        {
            Socket = new SocketManagerNS.SocketManager($"{ip}:{port}");


            try
            {
                //TcpModbus.Connect(ip, port);
                Socket.Connect(false);
            }
            catch
            {
                IsError = true;
            }

            if (Socket.IsConnected)
                return true;
            else
                return false;
        }

        public void Disconnect() => Socket?.Disconnect();

        public bool GetBool(int addr)
        {
            try
            {
                byte[] b = Socket.WriteRead(new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiRequest(SimpleModbus.PublicFunctionCodes.ReadDiscreteInput, addr, 1)).Message);
                return ((SimpleModbus.ADU_MultiResponse)new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiResponse(), b).PDU).Bool;
            }

            catch (Exception)
            {
                IsError = true;
                return false;
            }
        }

        public int GetInt16(int addr)
        {

            try
            {
                byte[] b = Socket.WriteRead(new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiRequest(SimpleModbus.PublicFunctionCodes.ReadInputRegister, addr, 1)).Message);
                return ((SimpleModbus.ADU_MultiResponse)new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiResponse(), b).PDU).Int16;
            }
            catch (Exception)
            {
                IsError = true;
                return 0;
            }

        }

        public int GetInt32(int addr)
        {
            try
            {
                byte[] b = Socket.WriteRead(new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiRequest(SimpleModbus.PublicFunctionCodes.ReadInputRegister, addr, 2)).Message);
                return ((SimpleModbus.ADU_MultiResponse)new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiResponse(), b).PDU).Int32;
            }
            catch (Exception)
            {
                IsError = true;
                return 0;
            }

        }

        public float GetFloat(int addr)
        {
            try
            {
                byte[] b = Socket.WriteRead(new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiRequest(SimpleModbus.PublicFunctionCodes.ReadInputRegister, addr, 2)).Message);
                return ((SimpleModbus.ADU_MultiResponse)new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiResponse(), b).PDU).Float;
            }
            catch (Exception)
            {
                IsError = true;
                return 0.0f;
            }

        }

        public string GetString(int addr)
        {
            return null;
        }

        public bool SetBool(int addr, bool val)
        {
            try
            {
                byte[] b = Socket.WriteRead(new SimpleModbus.MBAP(new SimpleModbus.ADU_CoilRequest(SimpleModbus.PublicFunctionCodes.WriteSingleCoil, addr, val)).Message);
                return ((SimpleModbus.ADU_CoilResponse)new SimpleModbus.MBAP(new SimpleModbus.ADU_CoilResponse(), b).PDU).IsExceptionFunctionCode;
            }
            catch (Exception)
            {
                IsError = true;
                return false;
            }
        }

        public bool SetInt16(int addr, int value)
        {
            try
            {
                byte[] b = Socket.WriteRead(new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiRequest(SimpleModbus.PublicFunctionCodes.WriteSingleRegister, addr, value)).Message);
                SimpleModbus.ADU_MultiResponse res = ((SimpleModbus.ADU_MultiResponse)new SimpleModbus.MBAP(new SimpleModbus.ADU_MultiResponse(), b).PDU);
                return res.IsExceptionFunctionCode;
            }
            catch (Exception)
            {
                IsError = true;
                return false;
            }

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Socket?.Disconnect();

                if (disposing)
                {
                    Socket?.Dispose();
                }

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
