using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIConnector.Model
{
    public class _GCPOAuth2ClientSecretKeyObjectWeb
    {
        public string? client_id { get; set; }
        public string? project_id { get; set; }
        public string? auth_uri { get; set; }
        public string? token_uri { get; set; }
        public string? auth_provider_x509_cert_url { get; set; }
        public string? client_secret { get; set; }
        public string[]? redirect_uris { get; set; }
        public string[]? javascript_origins { get; set; }
    
    }


    public class GCPOAuth2ClientSecretKeyObject { 
        public _GCPOAuth2ClientSecretKeyObjectWeb? web { get; set; }
    }

}
