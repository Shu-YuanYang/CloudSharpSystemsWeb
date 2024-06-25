using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models.Mongo
{
    public class O_TEAM_NOTE_PERMISSIONS
    {
        public List<string>? EDIT { get; set; }
        public List<string>? REMOVE { get; set; }
        public List<string>? COMPLETE { get; set; }
    }
}
