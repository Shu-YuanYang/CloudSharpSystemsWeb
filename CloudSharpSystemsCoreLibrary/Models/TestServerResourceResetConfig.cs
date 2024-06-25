using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSharpSystemsCoreLibrary.Models
{
    public class TestServerResourceResetConfig
    {
        public string? server_host_ip { get; set; }
        public int capacity { get; set; }
        public float preset_error_rate { get; set; }
        public string? reset_status { get; set; }
        public string? server_host_message { get; set; }

    }
}
