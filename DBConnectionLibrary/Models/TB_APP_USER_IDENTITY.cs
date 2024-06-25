using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Table("TB_APP_USER_IDENTITY", Schema = DB_SCHEMA.AUTH)]
    public class TB_APP_USER_IDENTITY
    {
        [Key]
        public string? IDENTITY_PROVIDER { get; set; }
        [Key]
        public string? USERNAME { get; set; }
        public string? USERID { get; set; }
	    public string? USERNAME_ALIAS { get; set; }
        public string? LANGUAGE_CODE { get; set; }
        public char IS_ENABLED { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }


}
