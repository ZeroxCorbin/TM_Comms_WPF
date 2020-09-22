using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TM_Comms_WPF
{
    public class TM_Comms_EthernetSlave
    {
        public TM_Comms_EthernetSlave(string item_Value, HEADERS header = HEADERS.TMSVR, string transactionID = "1", MODES mode = MODES.STRING)
        {
            Header = header;
            TransactionID = transactionID;
            Mode = mode;
            Item_Value = item_Value;
        }
        public const string StartChar = "$";
        public const string SeparatorChar = ",";
        public const string ChecksumChar = "*";
        public const string EndChar = "\r\n";
        public enum HEADERS
        { 
            TMSVR, //External Script
            CPERR  //Communication data error
        }

        public HEADERS Header { get; set; }
        public string HeaderString => Header.ToString(); 

        public enum MODES
        {
            STRING_RESPONSE,
            BINARY,
            STRING,
            JSON
        }
        public string TransactionID { get; private set; } = "1";
        public MODES Mode { get; private set; } = MODES.STRING;
        public string Item_Value { get; private set; } = string.Empty;


        public string Data => $"{TransactionID}{SeparatorChar}{Mode:D}{SeparatorChar}{Item_Value}";

        public int DataLength
        {
            get
            {
                if(Header == HEADERS.TMSVR)
                    return Data.Length;
                else
                    return 2;
            }
        }

        public byte Checksum => CalCheckSum();
        public string ChecksumString=> CalCheckSum().ToString("X2");

        public string Message
        {
            get
            {
                if(Header == HEADERS.TMSVR)
                    return StartChar + HeaderString + SeparatorChar + DataLength.ToString() + SeparatorChar + Data + SeparatorChar + ChecksumChar + ChecksumString + EndChar;
                return StartChar + HeaderString + SeparatorChar + DataLength.ToString() + SeparatorChar + "00" + SeparatorChar + ChecksumChar + ChecksumString + EndChar;
            }
        }

        private byte CalCheckSum()
        {
            Byte _CheckSumByte = 0x00;

            Byte[] bData;
            if (Header == HEADERS.TMSVR)
                bData = Encoding.ASCII.GetBytes(HeaderString + SeparatorChar + DataLength.ToString() + SeparatorChar + Data + SeparatorChar);
            else
                bData = Encoding.ASCII.GetBytes(HeaderString + SeparatorChar + DataLength.ToString() + SeparatorChar + "00" + SeparatorChar);
            
            for (int i = 0; i < bData.Length; i++)
                _CheckSumByte ^= bData[i];
            return _CheckSumByte;
        }
    }
}
