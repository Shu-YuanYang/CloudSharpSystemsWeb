using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CloudSharpLimitedCentralWeb.Models
{
    public class TestClientLoadedConnectionData
    {
        //public string? CLIENT_IP { get; set; }
        public string? thread_id { get; set; }
        public string? location { get; set; }
        public int resource_unit { get; set; }
        public DateTime requested_time { get; set; }
        public List<TestClientLoadedConnectionItem>? request_items { get; set;}
    }
}
