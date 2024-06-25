using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{
    public class IPGeoLocationObject
    {
        public string? status { get; set; } // "success" or else
        public string? country { get; set; } // "United States"
        public string? countryCode { get; set; } // "US"
        public string? region { get; set; } // "TX"
        public string? regionName { get; set; } // "Texas"
        public string? city { get; set; } // "Houston"
        public string? zip { get; set; } // "77003"
        public float lat { get; set; } // 29.752
        public float lon { get; set; } // -95.343
        public string? timezone { get; set; } // "America/Chicago"
        public string? isp { get; set; } // "Comcast Cable Communications, LLC"
        public string? org { get; set; } // "Comcast Cable Communications, Inc."
        public string? AS { get; set; } // "AS33662 Comcast Cable Communications, LLC"
        public string? query { get; set; } //"98.198.215.249"

    }
}
