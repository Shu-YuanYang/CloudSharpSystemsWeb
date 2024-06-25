using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class T_WEBSITE_MENU_ITEM
    {
        public string? MENU_DISPLAY_NAME { get; set; }
        public int MENU_RANKING { get; set; }
        public string? ITEM_NAME { get; set; }
        public string? DISPLAY_NAME { get; set; }
        public string? ROUTE_TYPE { get; set; }
        public string? ROUTE { get; set; }
        public string? ICON { get; set; }
        public int RANKING { get; set; }
    }
}
