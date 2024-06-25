using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{
    public class GCPServiceAccountSecretKeyObject
    {
        public string? type { get; set; }
        public string? project_id { get; set; }
        public string? private_key_id { get; set; }
        public string? private_key { get; set; }
        public string? client_email { get; set; }
        public string? client_id { get; set; }
        public string? auth_uri { get; set; }
        public string? token_uri { get; set; }
        public string? auth_provider_x509_cert_url { get; set; }
        public string? client_x509_cert_url { get; set; }
        public string? universe_domain { get; set; }
    }

}
