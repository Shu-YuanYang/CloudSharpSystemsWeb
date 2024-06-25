using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryClassLibrary.Network
{
    public class ReverseProxyHttpHeader 
    {
        public const string Client_IP = "Client_IP";
        public const string Trace_Identifier = "Trace_Identifier";
        public const string Requested_Time = "Requested_Time";
        public const string Authorization = "Authorization";
        public const string Origin = "Origin";
    }

    public class ReverseProxyConfig 
    {
        public const string REVERSE_PROXY_CONFIG = "ReverseProxyConfig";
        public const string FORWARDED_PATHS = "ForwardedPaths";
        public const string RECONFIG_PATHS = "ReconfigPaths";
    }
}
