using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class T_HOST_LATENCY_STATISTICS
    {
        public DateTime END_TIME { get; set; }
        public double MIN_LATENCY { get; set; }
        public double AVG_LATENCY { get; set; }
        public double MAX_LATENCY { get; set; }
    }
}
