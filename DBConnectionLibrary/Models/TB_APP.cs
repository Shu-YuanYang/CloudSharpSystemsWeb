using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DBConnectionLibrary.Models
{
    [Table("TB_APP", Schema = DB_SCHEMA.APPLICATIONS)]
    public class TB_APP
    {
        [Key]
        public string? APP_ID { get; set; }
        public string? APP_DESCRIPTION { get; set; }
        public char IS_ENABLED { get; set; }
        public string? OWNED_BY { get; set; }
        public string? CREATED_BY { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_DATE { get; set; }
        
    }
}
