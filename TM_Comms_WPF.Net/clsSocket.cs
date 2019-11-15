using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace clsSocketNS
{

    public class clsSocket : IDisposable
    {

        public class clsSocketEventArgs : EventArgs
        {
            public string Message { get; }
            public clsSocketEventArgs(string msg)
            {
                Message = msg;
            }
        }

        public delegate void DataReceivedEventHandler(object sender, clsSocketEventArgs data);
        public event DataReceivedEventHandler DataReceived;

        public delegate void ClosedEventHandler(object sender, clsSocketEventArgs data);
        public event ClosedEventHandler Closed;

        public string ConnectionString { get; private set; }
        public string IPAddress
        {
            get
            {
                if (ConnectionString.Count(c => c == ':') != 1) return string.Empty;
                return ConnectionString.Split(':')[0];
            }
        }
        public int Port
        {
            get
            {
                if (ConnectionString.Count(c => c == ':') != 1) return 0;
                return int.Parse(ConnectionString.Split(':')[1]);
            }
        }
        public string Password
        {
            get
            {
                if (ConnectionString.Count(c => c == ':') != 2) return string.Empty;
                return ConnectionString.Split(':')[2];
            }
        }

        private TcpClient Client;
        private NetworkStream ClientStream;
        public int BufferSize { get; private set; } = 1024;
        public int SendTimeout { get; private set; } = 500;
        public int RecieveTimeout { get; private set; } = 500;

        public bool IsConnected { get { return (Client != null) ? Client.Connected : false; } }
        public bool IsRunning { get; private set; } = true;
        public int UpdateRate { get; private set; } = 50;
        private object LockObject = new object();
        public string GenerateConnectionString(string ip, int port, string pass) => ip + ":" + port.ToString() + pass;
        public static bool ValidateConnectionString(string connectionString)
        {
            if (connectionString.Count(c => c == ':') != 2) return false;
            string[] spl = connectionString.Split(':');

            if (!System.Net.IPAddress.TryParse(spl[0], out IPAddress ip)) return false;

            if (!int.TryParse(spl[1], out int port)) return false;

            if (spl[2].Length <= 0) return false;

            return true;
        }



        public clsSocket(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public bool Connect(bool withTimeout)
        {
            try
            {
                Client = new TcpClient
                {
                    SendTimeout = SendTimeout,
                    ReceiveTimeout = RecieveTimeout
                };

                if (withTimeout)
                {
                    if (!ConnectWithTimeout(3))
                        return false;
                }

                else
                    Client.Connect(IPAddress, Port);
                
                ClientStream = Client.GetStream();
                
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool Disconnect()
        {
            StopRecieveAsync();
            try
            {
                if (ClientStream != null)
                {
                    ClientStream.Close();
                    Client.Close();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool ConnectWithTimeout(int timeout)
        {
            bool connected = false;
            IAsyncResult ar = Client.BeginConnect(IPAddress, Port, null, null);
            System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
            try
            {
                if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeout), false))
                {
                    Client.Close();
                    connected = false;
                }
                else
                {
                    connected = true;
                }
                if (Client.Client != null)
                    Client.EndConnect(ar);
            }
            finally
            {
                wh.Close();
            }
            return connected;
        }

        public string Read()
        {
            int timeout = 45000; //ms
            Stopwatch sw = new Stopwatch();
            StringBuilder completeMessage = new System.Text.StringBuilder();

            try
            {
                sw.Start();
                lock (LockObject)
                {
                    if (ClientStream.CanRead && ClientStream.DataAvailable)
                    {
                        byte[] readBuffer = new byte[BufferSize];
                        int numberOfBytesRead = 0;

                        // Fill byte array with data from clsSocket1 stream
                        numberOfBytesRead = ClientStream.Read(readBuffer, 0, readBuffer.Length);

                        // Convert the number of bytes received to a string and
                        // concatenate to complete message
                        completeMessage.AppendFormat("{0}", System.Text.Encoding.ASCII.GetString(readBuffer, 0, numberOfBytesRead));

                        sw.Stop();
                        if (sw.ElapsedMilliseconds >= timeout)
                            throw new TimeoutException();
                    }
                }
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return completeMessage.ToString();
        }
        public string Read(string endString)
        {
            int timeout = 45000; //ms
            Stopwatch sw = new Stopwatch();
            StringBuilder completeMessage = new System.Text.StringBuilder();

            // Read until find the given string argument or hit timeout
            sw.Start();
            do
            {
                // Convert the number of bytes received to a string and
                // concatenate to complete message
                completeMessage.AppendFormat("{0}", Read());
            }
            while (!completeMessage.ToString().Contains(endString) &&
                   sw.ElapsedMilliseconds < timeout);
            sw.Stop();

            if (sw.ElapsedMilliseconds >= timeout)
                throw new TimeoutException();

            return completeMessage.ToString();
        }
        public string ReadLine()
        {
            int timeout = 45000; //ms
            Stopwatch sw = new Stopwatch();
            StringBuilder completeMessage = new System.Text.StringBuilder();

            try
            {
                sw.Start();
                lock (LockObject)
                {
                    if (ClientStream.CanRead && ClientStream.DataAvailable)
                    {
                        int readBuffer = 0;
                        char singleChar = '~';

                        while (singleChar != '\n' && singleChar != '\r')
                        {
                            // Read the first byte of the stream
                            readBuffer = ClientStream.ReadByte();

                            //Start converting and appending the bytes into a message
                            singleChar = (char)readBuffer;

                            completeMessage.AppendFormat("{0}", singleChar.ToString());

                            if (sw.ElapsedMilliseconds >= timeout)
                                throw new TimeoutException();

                        }
                        sw.Stop();
                    }
                }
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            return completeMessage.ToString();
        }
        public string ReadMessage()
        {
            int timeout = 45000; //ms
            Stopwatch sw = new Stopwatch();
            StringBuilder completeMessage = new System.Text.StringBuilder();

            sw.Start();
            // Read until find the given string argument or hit timeout
            do
            {
                // Convert the number of bytes received to a string and
                // concatenate to complete message
                completeMessage.AppendFormat("{0}", Read());
                Thread.Sleep(5);
            }
            while (ClientStream.DataAvailable &&
                   sw.ElapsedMilliseconds < timeout);
            sw.Stop();

            if (sw.ElapsedMilliseconds >= timeout)
                throw new TimeoutException();

            return completeMessage.ToString();
        }

        private void ReadComplete(IAsyncResult iar)

        {

            if (!IsRunning) return;

            byte[] buffer = (byte[])iar.AsyncState;
            int BytesAvailable = ClientStream.EndRead(iar);

            DataReceived?.Invoke(new object(), new clsSocketEventArgs(BytesToString(buffer)));

            if (DetectConnection())
            {
                ClientStream.BeginRead(buffer, 0, buffer.Length, ReadComplete, buffer);
            }
            else
            {
                Closed.Invoke(new object(), new clsSocketEventArgs("Read Detected Closed Connection"));
            }
        }

        private bool DetectConnection()
        {
            // Detect if client disconnected
            if (Client.Client.Poll(0, SelectMode.SelectRead))
            {
                byte[] buff = new byte[1];
                if (Client.Client.Receive(buff, SocketFlags.Peek) == 0)
                {
                    // Client disconnected
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }
        public bool Write(string msg)
        {
            byte[] buffer_ot = new byte[BufferSize];
            msg += '\r';
            try
            {
                lock (LockObject)
                {
                    StringToBytes(msg, ref buffer_ot);
                    ClientStream.Write(buffer_ot, 0, buffer_ot.Length);
                    bzero(buffer_ot);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }
        public bool Write(string msg, int waitTime)
        {
            if (Write(msg))
            {
                Thread.Sleep(waitTime);
                return true;
            }
            else
                return false; ;
        }

        public void StartRecieveAsync(int rate = 20)
        {
            IsRunning = true;

            byte[] buffer = new byte[10000];

            ClientStream.BeginRead(buffer, 0, buffer.Length, ReadComplete, buffer);
        }
        public void StopRecieveAsync()
        {
            IsRunning = false;
            Thread.Sleep(UpdateRate + 100);
        }



        public string[] MessageParse(string message)
        {
            string[] messages = message.Split('\n', '\r');

            List<string> _messages = new List<string>();

            foreach (string item in messages)
            {
                if (!String.IsNullOrEmpty(item))
                {
                    _messages.Add(item);
                }
            }
            messages = _messages.ToArray();
            return messages;
        }



        private void bzero(byte[] buff)
        {
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = 0;
            }
        }
        public byte[] StringToBytes(string msg)
        {
            byte[] buffer = new byte[msg.Length];
            buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
            return buffer;
        }
        public void StringToBytes(string msg, ref byte[] buffer)
        {
            bzero(buffer);
            buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(msg);
        }

        public string BytesToString(byte[] buffer)
        {
            string msg = System.Text.ASCIIEncoding.ASCII.GetString(buffer, 0, buffer.Length);
            return msg;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Client?.Dispose();
                    ClientStream?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~clsSocket() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

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