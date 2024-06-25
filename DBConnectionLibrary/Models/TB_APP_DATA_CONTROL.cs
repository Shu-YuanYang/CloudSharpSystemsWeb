using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    public class TB_APP_DATA_CONTROL
    {
        [Key]
        public string? APP_ID { get; set; }
        [Key]
        public string? CONTROL_NAME { get; set; }
        [Key]
        public string? CONTROL_TYPE { get; set; }
        [Key]
        public string? CONTROL_LEVEL { get; set; }
        [Key]
        public string? CONTROL_VALUE { get; set; }
        [Key]
        public string? CONTROL_NOTE { get; set; } 
        public char IS_ENABLED { get; set; }
        public string? EDIT_BY { get; set; }
        public DateTime EDIT_DATE { get; set; }
    }
}
