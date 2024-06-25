using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{

    [Table("TB_USER_SESSION", Schema = DB_SCHEMA.NETWORK)]
    public class TB_USER_SESSION
    {
        [Key]
        public string? SESSION_ID { get; set; }
        public string? CLIENT_IP { get; set; }
        public string? THREAD_ID { get; set; }
        public string? HOST_IP { get; set; }
        public int RESOURCE_UNIT { get; set; }
        public string? CLIENT_LOCATION { get; set; }
        public DateTime REQUESTED_TIME { get; set; }
        public int RESOURCE_SIZE { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
        public char IS_VALID { get; set; }
        [NotMapped]
        public List<TB_USER_SESSION_ITEM>? SESSION_ITEMS { get; set; }
    }
}
