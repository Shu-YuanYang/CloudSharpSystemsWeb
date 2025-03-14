using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class T_CENTRAL_SYSTEM_LOG_VOLUME
    {
        public string? APP_ID { get; set; }
        public DateTime LOG_DATE { get; set; }
        public int LOG_COUNT { get; set; }   
	}
}
