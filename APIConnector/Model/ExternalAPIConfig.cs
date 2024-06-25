using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{

    public class ExternalAPIPathConfig {
        public string? path { get; set; }
        public string? method { get; set; }
        public Object? default_body { get; set; }
    }

    public class ExternalAPIConfig
    {
        public string? url { get; set; }
        public Dictionary<string, ExternalAPIPathConfig>? api { get; set; }
    }

    public class ExternalAPIMap {
        public ExternalAPIConfig? GoogleAPI { get; set; }
        public ExternalAPIConfig? CloudSharpMicroService { get; set; }
        public ExternalAPIConfig? CloudSharpVisualDataDashboard { get; set; }
    }

}
