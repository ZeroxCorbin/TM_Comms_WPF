using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commands
{
    //Usage: ping [-t] [-a] [-n count] [-l size] [-f] [-i TTL] [-v TOS] [-r count] [-s count] [[-j host-list] | [-k host-list]] [-w timeout] [-R] [-S srcaddr] [-4] [-6 target_name]

    public class PingCommand
    {

        public string GetCommandLineArguments()
        {
            StringBuilder args = new StringBuilder();

            if (Continuous) args.Append(Continuous_Command);
            else args.Append(EchoRequestCount_Command);
            args.Append(ResolveAddressToHostname_Command);
            args.Append(EchoRequestPacketSize_Command);
            args.Append(ICMPNoFragment_Command);
            args.Append(TimeToLive_Command);
            args.Append(NumberOfHops_Command);
            args.Append(InternetTimestampCount_Command);
            args.Append(EchoTimeout_Command);
            args.Append(TraceRoundTripPath_Command);
            args.Append(SourceAddress_Command);
            args.Append(IPV4Only_Command);
            args.Append(IPV6Only_Command);
            args.Append(Target_Command);

            return args.ToString();
        }

        public void Run(string target)
        {
            Target = target;
            System.Diagnostics.Process.Start("cmd", "/K \"echo ping " + GetCommandLineArguments() + " & ping " + GetCommandLineArguments() + "\"");
        }

        public string Target { get; set; } = string.Empty;
        public string Target_Command
        {
            get { if (!string.IsNullOrEmpty(Target)) return Target; else return string.Empty; }
        }

        public bool Continuous { get; set; } = false;
        public string Continuous_Command
        {
            get { if (Continuous) return "-t "; else return string.Empty; }
        }

        public bool ResolveAddressToHostname { get; set; } = false;
        public string ResolveAddressToHostname_Command
        {
            get { if (ResolveAddressToHostname) return "-a "; else return string.Empty; }
        }


        private uint _EchoRequestCount = 4;
        public uint EchoRequestCount
        {
            get
            {
                return _EchoRequestCount;
            }
            set
            {
                if (value > 4294967295) _EchoRequestCount = 4294967295;
                else _EchoRequestCount = value;
            }
        }
        public string EchoRequestCount_Command
        {
            get { if (_EchoRequestCount > 0) return "-n " + _EchoRequestCount.ToString() + " "; else return string.Empty; }
        }


        private ushort _EchoRequestPacketSize = 32;
        public ushort EchoRequestPacketSize
        {
            get
            {
                return _EchoRequestPacketSize;
            }
            set
            {
                if(value > 65527) _EchoRequestPacketSize = 65527;
                else _EchoRequestPacketSize = value;
            }
        }
        public string EchoRequestPacketSize_Command
        {
            get { if (_EchoRequestPacketSize > 0) return "-l " + _EchoRequestPacketSize.ToString() + " "; else return string.Empty; }
        }

        public bool ICMPNoFragment { get; set; } = false;
        public string ICMPNoFragment_Command
        {
            get { if (ICMPNoFragment) return "-f "; else return string.Empty; }
        }

        public byte TimeToLive { get; set; } = 32;
        public string TimeToLive_Command
        {
            get { if (TimeToLive > 0) return "-i " + TimeToLive.ToString() + " "; else return string.Empty; }
        }


        public byte NumberOfHops { get; set; } = 0;
        public string NumberOfHops_Command
        {
            get { if (NumberOfHops > 0) return "-r " + NumberOfHops.ToString() + " "; else return string.Empty; }
        }


        private byte _InternetTimestampCount = 0;
        public byte InternetTimestampCount
        {
            get
            {
                return _InternetTimestampCount;
            }
            set
            {
                if (value > 4) _InternetTimestampCount = 4;
                else _InternetTimestampCount = value;
            }
        }
        public string InternetTimestampCount_Command
        {
            get { if (_InternetTimestampCount > 0) return "-s " + _InternetTimestampCount.ToString() + " "; else return string.Empty; }
        }


        private uint _EchoTimeout = 4000;
        public uint EchoTimeout
        {
            get
            {
                return _EchoTimeout;
            }
            set
            {
                if (value > 4294967295) _EchoTimeout = 4294967295;
                else _EchoTimeout = value;
            }
        }
        public string EchoTimeout_Command
        {
            get { if (_EchoTimeout > 0) return "-w " + _EchoTimeout.ToString() + " "; else return string.Empty; }
        }

        public bool TraceRoundTripPath { get; set; } = false;
        public string TraceRoundTripPath_Command
        {
            get { if (TraceRoundTripPath) return "-R "; else return string.Empty; }
        }

        public string SourceAddress { get; set; } = string.Empty;
        public string SourceAddress_Command
        {
            get { if (!string.IsNullOrEmpty(SourceAddress)) return "-S " + SourceAddress + " "; else return string.Empty; }
        }

        public bool IPV4Only { get; set; } = false;
        public string IPV4Only_Command
        {
            get { if (IPV4Only) return "-4 "; else return string.Empty; }
        }

        public bool IPV6Only { get; set; } = false;
        public string IPV6Only_Command
        {
            get { if (IPV6Only) return "-6 "; else return string.Empty; }
        }


    }
}
