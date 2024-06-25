using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{
    public enum RESTAPIType { 
        GET, POST
    }

    public class RESTAPIInputModel
    {
        public string? URL { get; set; }
        public string? ContentType { get; set; }
        [DefaultValue(RESTAPIType.GET)]
        public RESTAPIType APIType { get; set; }
        public string? Parameters { get; set; }
        public Object? InputObject { get; set; }
        public string? AccessToken { get; set; }
    }
}
