using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DBConnectionLibrary.Models
{
    [Table("TB_WEBSITE_MENU_ITEM", Schema = DB_SCHEMA.INTERFACES)]
    public class TB_WEBSITE_MENU_ITEM
    {
        [Key]
        public string? HEADER_ID { get; set; }
        [Key]
        public string? ITEM_NAME { get; set; }
        public string? DISPLAY_NAME { get; set; }
        public string? ROUTE_TYPE { get; set; }
        public string? ROUTE { get; set; }
        public string? ICON { get; set; }
        public int RANKING { get; set; }
        public char IS_ENABLED { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_TIME { get; set; }
    }
}
