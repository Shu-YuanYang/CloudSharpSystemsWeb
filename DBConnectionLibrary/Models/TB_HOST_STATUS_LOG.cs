using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Table("TB_HOST_STATUS_LOG", Schema = DB_SCHEMA.NETWORK)]
    public class TB_HOST_STATUS_LOG
    {
        [Key]
        public string? LOG_ID { get; set; }
        public string? HOST_IP { get; set; }
        public string? HOST_STATUS { get; set; }
        public string? TRACE_ID { get; set; }
        public string? RECORD_TYPE { get; set; }
        public string? RECORD_MESSAGE { get; set; }
        public double ERROR_RATE { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
        public double LATENCY { get; set; }
    }
}
