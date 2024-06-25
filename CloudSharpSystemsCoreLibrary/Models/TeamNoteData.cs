using DBConnectionLibrary.Models.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSharpSystemsCoreLibrary.Models
{
    public class TeamNoteUserPermissions {
        public bool can_edit { get; set; }
        public bool can_remove { get; set; }
        public bool can_complete { get; set; }
    }

    public class TeamNoteData
    {
        public string? note_id { get; set; }
        public string? team_name { get; set; }
        public string? sender_name { get; set; }
        public string? priority { get; set; }
        //public int priority_number { get; set; }
        public string? title { get; set; }
        public string? message { get; set; }
        public string? last_edited_by { get; set; }
        public DateTime last_edited_time { get; set; }
        public string? note_hash { get; set; }
        public O_TEAM_NOTE_PERMISSIONS? permissions { get; set; }
        public TeamNoteUserPermissions? user_permissions { get; set; }
        //public string? status { get; set; }
        public string? status_code { get; set; }
    }
}
