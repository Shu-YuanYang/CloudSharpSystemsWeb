using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{
    public class GoogleAPIOAuth2LoginCodeObject
    {
        public string? code { get; set; }
        public string? scope { get; set; }
        public string? authuser { get; set; }
        public string? prompt { get; set; }
    }
}
