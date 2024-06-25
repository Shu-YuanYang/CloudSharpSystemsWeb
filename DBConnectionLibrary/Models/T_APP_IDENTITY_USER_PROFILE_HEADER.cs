using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
    [Keyless]
    public class T_APP_IDENTITY_USER_PROFILE_HEADER
    {
        public string? FIRST_NAME { get; set; }
        public string? LAST_NAME { get; set; }
        public string? NAME_ALIAS { get; set; }
        public string? PHONE_NUMBER { get; set; }
        public string? PROFILE_PICTURE { get; set; }
        public string? NOTES { get; set; }
        public string? IDENTITY_PROVIDER { get; set; }
        public string? USERNAME_ALIAS { get; set; }
        public string? LANGUAGE_CODE { get; set; }
    }


}
