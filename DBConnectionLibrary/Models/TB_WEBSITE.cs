using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


namespace DBConnectionLibrary.Models
{
    [Table("TB_WEBSITE", Schema = DB_SCHEMA.NETWORK)]
    public class TB_WEBSITE
    {
        [Key]
        public string? SITE_ID { get; set; }
        public string? APP_ID { get; set; }
        public string? DOMAIN_NAME { get; set; }
        public string? SITE_NAME { get; set; }
        public string? LOAD_BALANCING_ALGORITHM { get; set; }
        public string? THEME_COLOR { get; set; }
        public char IS_ENABLED { get; set; }
        public string? OWNED_BY { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
