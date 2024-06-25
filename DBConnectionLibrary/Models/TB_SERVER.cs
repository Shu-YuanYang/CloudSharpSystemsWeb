using Microsoft.Identity.Client.Extensions.Msal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Table("TB_SERVER", Schema = DB_SCHEMA.PRODUCTS)]
    public class TB_SERVER
    {
        [Key]
        public string? SERIAL_NO { get; set; } 
        public string? STATUS { get; set; } 
        public int NET_LOAD_CAPACITY { get; set; }
        public string? SERVER_SPEC { get; set; }
        public string? CPU { get; set; } 
        public string? RAM { get; set; }
        public string? STORAGE { get; set; }
        public string? PSU { get; set; } 
        public string? FAN { get; set; } 
        public DateTime REGISTRATION_DATE { get; set; } 
        public DateTime? LAST_SERVICE_DATE { get; set; } 
        public string? OWNED_BY { get; set; }   
        public string? LOCATION_CODE { get; set; }
        public string? RACK_CODE { get; set; }  
        public string? EDIT_BY { get; set; } 
        public DateTime EDIT_TIME { get; set; }

    }
}
