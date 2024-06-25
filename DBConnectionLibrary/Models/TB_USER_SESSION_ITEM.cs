using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Table("TB_USER_SESSION_ITEM", Schema = DB_SCHEMA.NETWORK)]
    public class TB_USER_SESSION_ITEM
    {
        [Key]
        public string? SESSION_ID { get; set; }
        [Key]
        public string? ITEM_NAME { get; set; }
        public string? ITEM_DESCRIPTION { get; set; }
        public int ITEM_SIZE { get; set; }
        public string? ITEM_ROUTE { get; set; } // can store identity provider
        public string? ITEM_POLICY { get; set; } // can store identity access token
        public DateTime? EXPIRATION_TIME { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
