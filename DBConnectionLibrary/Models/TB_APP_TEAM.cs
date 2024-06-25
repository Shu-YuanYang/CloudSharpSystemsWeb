using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DBConnectionLibrary.Models
{
    [Table("TB_APP_TEAM", Schema = DB_SCHEMA.AUTH)]
    public class TB_APP_TEAM
    {
        [Key]
        public string? TEAM_ID { get; set; }
        public string? APP_ID { get; set; }
        public string? TEAM_NAME { get; set; }
        public string? TEAM_DESCRIPTION { get; set; }
        public string? PROFILE_PICTURE { get; set; }
        public string? PRIMARY_CONTACT { get; set; }
        public char IS_ENABLED { get; set; }
        public string? OWNED_BY { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
