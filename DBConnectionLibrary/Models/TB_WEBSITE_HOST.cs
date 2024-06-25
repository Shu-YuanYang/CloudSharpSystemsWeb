using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Table("TB_WEBSITE_HOST", Schema = DB_SCHEMA.NETWORK)]
    public class TB_WEBSITE_HOST
    {
        [Key]
        public string? HOST_IP { get; set; }
        public string? PORT { get; set; }
        public string? SITE_ID { get; set; }
        public string? SERIAL_NO { get; set; }
        public string? STATUS { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
        public string? ERROR_MEASUREMENT_ALGORITHM { get; set; }
        public double ERROR_RATE { get; set; }
        public DateTime MEASURED_TIME { get; set; }
    }


}
