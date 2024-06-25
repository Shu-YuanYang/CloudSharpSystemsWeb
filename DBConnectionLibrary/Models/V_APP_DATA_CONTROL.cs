using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class V_APP_DATA_CONTROL
    {
        public string? APP_ID { get; set; } 
        public char IS_APP_ENABLED { get; set; }  
        public string? CONTROL_NAME { get; set; }
        public string? CONTROL_TYPE { get; set; } 
        public string? CONTROL_LEVEL { get; set; }
        public string? CONTROL_VALUE { get; set; } 
        public string? CONTROL_NOTE { get; set; } 
        public char IS_CONTROL_ENABLED { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_DATE { get; set; }
    }
}
