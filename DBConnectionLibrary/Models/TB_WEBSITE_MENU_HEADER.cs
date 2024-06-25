using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{

    /*
     
    public class TB_USER_SESSION
    {
        
    }
     */
    [Table("TB_WEBSITE_MENU_HEADER", Schema = DB_SCHEMA.INTERFACES)]
    public class TB_WEBSITE_MENU_HEADER
    {
        [Key]
        public string? HEADER_ID { get; set; }
        public string? SITE_ID { get; set; }
        public string? PARENT_HEADER_ID { get; set; }
        public string? USER_ID { get; set; }
        public string? MENU_NAME { get; set; }
        public string? DISPLAY_NAME { get; set; }
        public char IS_ENABLED { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
        [NotMapped]
        public List<TB_WEBSITE_MENU_ITEM>? MENU_ITEMS { get; set; }
    }
}
