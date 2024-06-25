using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class T_APP_USER_TEAM_STATISTICS
    {
        public string? TEAM_ID { get; set; }
        public string? APP_ID { get; set; }
        public string? TEAM_NAME { get; set; }
        public string? TEAM_DESCRIPTION { get; set; }
        public string? TEAM_PROFILE_PICTURE { get; set; }
        public string? USER_ID { get; set; }
        public string? FIRST_NAME { get; set; }
        public string? LAST_NAME { get; set; }
        public string? NAME_ALIAS { get; set; }
        public string? PHONE_NUMBER { get; set; }
        public string? USER_PROFILE_PICTURE { get; set; }
        public string? NOTES { get; set; }
        public string? LANGUAGE_CODE { get; set; }
    }
}
