using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSharpSystemsCoreLibrary.Models
{
    public class SystemHealthTraceRecord
    {
        public string? host_IP { get; set; }
        public string? port { get; set; }
        public string? system_status { get; set; }
        public string? trace_ID { get; set; }
        public string? message { get; set; }
        public string? recorded_by { get; set; }
        public double latency { get; set; }
    }
}
