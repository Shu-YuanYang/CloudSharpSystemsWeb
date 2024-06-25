using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Table("TB_CENTRAL_SYSTEM_LOG", Schema = DB_SCHEMA.APPLICATIONS)]
    public class TB_CENTRAL_SYSTEM_LOG
    {
        [Key]
        public string? LOG_ID { get; set; }
        public string? APP_ID { get; set; }
        public string? SYSTEM_NAME { get; set; }
        public string? TRACE_ID { get; set; }
	    public string? RECORD_TYPE { get; set; }
	    public string? RECORD_KEY { get; set; }
	    public string? RECORD_VALUE1 { get; set; }
	    public string? RECORD_VALUE2 { get; set; }
	    public string? RECORD_VALUE3 { get; set; }
	    public string? RECORD_VALUE4 { get; set; }
	    public string? RECORD_VALUE5 { get; set; }
	    public string? RECORD_MESSAGE { get; set; }
        public string? RECORD_NOTE { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
