using APIConnector.GoogleCloud;
using APIConnector.Model;
using CloudSharpSystemsCoreLibrary.Sessions;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace CloudSharpLimitedCentral.Controllers
{
    [Route("[controller]")]
    [ApiController]
    abstract public class TemplateController : ControllerBase
    {
        public const string APP_ID_CONFIG_KEY = "AppID";
        public const string SITE_CONFIG_KEY = "SiteConfig";
        public const string LAMBDA_ENV_KEY = "IsLambdaEnvironment";
        //public const string GCP_RESOURCE_CONFIG_KEY = "GCPResourceConfig";
        public const string EXTERNAL_API_CONFIG_KEY = "ExternalAPIConfig";
        

        protected readonly ILogger<TemplateController> _logger;
        protected readonly AppDBMainContext _app_db_main_context;
        protected readonly AppDBMongoContext _app_db_mongo_context;
        protected readonly IConfiguration _configuration;
        protected readonly GCPServiceAccountSecretKeyObject _gcp_service_account_key_obj;
        protected readonly GCPCredentialsHelper _gcp_credentials_helper;
        protected readonly ExternalAPIMap _external_api_map;
        protected readonly SessionManager _session_manager;
        private readonly string _APP_ID;
        private readonly string _SITE_ID;
        private readonly string _MY_PUBLIC_IP;
        

        public TemplateController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor)
        {
            _logger = logger;
            _app_db_main_context = appDBMainContext;
            _app_db_mongo_context = appDBMongoContext;
            _configuration = config;
            _APP_ID = config[TemplateController.APP_ID_CONFIG_KEY]!;
            _SITE_ID = config[TemplateController.SITE_CONFIG_KEY + ":" + this.GetType().Name]!;
            _external_api_map = config.GetSection(EXTERNAL_API_CONFIG_KEY).Get<ExternalAPIMap>();
            _session_manager = new SessionManager(appDBMainContext);
            _gcp_service_account_key_obj = GCPServiceAccountKeyAccessor.Value;
            _gcp_credentials_helper = new GCPCredentialsHelper(this._external_api_map);

            bool is_lambda = this._IsLambdaEnvironment();
            //if (is_lambda) _MY_PUBLIC_IP = "127.0.0.0"; // lambdda environment
            //else _MY_PUBLIC_IP = config["HostConfig"]!; //APIConnector.IPHelper.LocalIP.GetMyIP().Result!;
            _MY_PUBLIC_IP = config["HostConfig"]!;
        }


        protected string APP_ID { get => this._APP_ID; }
        protected string SITE_ID { get => this._SITE_ID; }
        protected string MY_PUBLIC_IP { get => this._MY_PUBLIC_IP; }



        private bool _IsLambdaEnvironment() {
            bool is_lambda = false;
            try { is_lambda = this._configuration.GetValue<bool>(LAMBDA_ENV_KEY); }
            catch { is_lambda = false; }
            return is_lambda;
        }

        

    }
}
