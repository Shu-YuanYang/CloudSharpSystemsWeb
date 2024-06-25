using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DBConnectionLibrary.Models
{
    [Table("TB_APP_USER", Schema = DB_SCHEMA.AUTH)]
    public class TB_APP_USER
    {
        [Key]
        public string? USERID { get; set; }
        public string? PASSWORD_SALT { get; set; }
        public string? PASSWORD_HASH { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? LAST_NAME { get; set; }
        public string? NAME_ALIAS { get; set; }
        public string? PHONE_NUMBER { get; set; }
        public string? PROFILE_PICTURE { get; set; }
        public string? NOTES { get; set; }
        public char IS_ENABLED { get; set; }
        public DateTime? LAST_LOGIN_TIME { get; set; }
        public DateTime CREATED_TIME { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
