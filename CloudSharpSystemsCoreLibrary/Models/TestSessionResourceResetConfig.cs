using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSharpSystemsCoreLibrary.Models
{
    public class TestSessionResourceResetConfig
    {
		public string? load_balancing_algorithm { get; set; }
        public int load_balancing_max_search_count { get; set; }
		public int request_batch_size { get; set; }
        public List<TestServerResourceResetConfig>? server_capacities { get; set; }

    }
}
