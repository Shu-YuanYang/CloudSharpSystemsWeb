using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBConnectionLibrary.Models
{
    [Table("TB_PROGRAM_STATUS", Schema = DB_SCHEMA.APPLICATIONS)]
    public class TB_PROGRAM_STATUS
    {
        [Key]
        public string? PROGRAM_ID { get; set; }
        [Key]
        public string? APP_ID { get; set; }
        public string? PROGRAM_TYPE { get; set; }
        public string? PROGRAM_STATUS { get; set; }
	    public string? LAST_TRACE_ID { get; set; }
	    public DateTime LAST_LOG_TIME { get; set; }
	    public string? NOTES { get; set; }
	    public int MAX_IDLE_INTERVAL { get; set; }
	    public string? RESOURCE_SEP { get; set; }
	    public string? RESOURCE { get; set; }
	    public string? PROGRAM_PATH { get; set; }
	    public string? EXECUTION_COMMAND { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
