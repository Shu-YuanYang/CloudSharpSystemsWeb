using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class T_SERVER_DETAIL
    {
        public string? SITE_ID { get; set; }
        public string? SERIAL_NO { get; set; }
        public string? HOST_IP { get; set; }
        public string? PORT { get; set; }
        public string? SERVER_STATUS { get; set; }
        public string? IP_STATUS { get; set; }
        public int NET_LOAD_CAPACITY { get; set; }
        public string? SERVER_SPEC { get; set; }
        public string? STORAGE { get; set; }
        public DateTime REGISTRATION_DATE { get; set; }
        public DateTime LAST_SERVICE_DATE { get; set; }
        public string? LOCATION_CODE { get; set; }
        public string? RACK_CODE { get; set; }
    }
}
