using APIConnector.IPHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{
    public class GoogleAPIOAuth2UserInfo
    {
        public string? email { get; set; }
        public string? family_name { get; set; }
        public string? given_name { get; set; }
        public string? id { get; set; }
        public string? locale { get; set; }
        public string? name { get; set; }
        public string? picture { get; set; }
        public bool verified_email { get; set; }
    }


}
