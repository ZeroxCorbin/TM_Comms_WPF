using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TM_Comms_WPF
{
    public class TM_Comms_EthernetSlave
    {
        public const string StartByte = "$";
        public const string Separator = ",";
        public const string ChecksumSign = "*";
        public const string EndBytes = "\r\n";
        public enum HEADERS
        { 
            TMSVR, //External Script
            CPERR  //Communication data error
        }

        private HEADERS _header;
        public HEADERS Header
        {
            get { return _header; }
            set { _header = value; }
        }

        private string _headerString
        {
            get { return _header.ToString(); }
        }

        public int Length
        {
            get
            {
                if(_header == HEADERS.TMSVR)
                    return  _scriptID.ToString().Length + Separator.Length + _data.Length;
                else
                    return "00".Length;
            }
        }

        private int _scriptID = 1; //new Random().Next();
        public int ScriptID
        {
            get { return _scriptID; }
        }

        private string _data;
        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public byte Checksum
        {
            get { return CalCheckSum(); }
        }

        private string _checksumString
        {
            get { return CalCheckSum().ToString("X2"); }
        }
        public string Message
        {
            get
            {
                if(_header == HEADERS.TMSVR)
                {
                    return StartByte + _headerString + Separator + Length.ToString() + Separator + _scriptID.ToString() + Separator + _data + Separator + ChecksumSign + _checksumString + EndBytes;
                }
                return StartByte + _headerString + Separator + Length.ToString() + Separator + "00" + Separator + ChecksumSign + _checksumString + EndBytes;
            }
            set
            {

            }

        }

        public TM_Comms_EthernetSlave()
        {
            this._header = HEADERS.TMSVR;
            this._data = "";
        }

        public TM_Comms_EthernetSlave(HEADERS header = HEADERS.TMSVR, string data = "")
        {
            this._header = header;
            this._data = data;

        }

        private byte CalCheckSum()
        {
            Byte _CheckSumByte = 0x00;

            Byte[] bData;
            if (_header == HEADERS.TMSVR)
                bData = Encoding.ASCII.GetBytes(_headerString + Separator + Length.ToString() + Separator + _scriptID.ToString() + Separator + _data + Separator);
            else
                bData = Encoding.ASCII.GetBytes(_headerString + Separator + Length.ToString() + Separator + "00" + Separator);
            
            for (int i = 0; i < bData.Length; i++)
                _CheckSumByte ^= bData[i];
            return _CheckSumByte;
        }
    }
}
