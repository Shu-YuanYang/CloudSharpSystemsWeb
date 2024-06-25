using APIConnector.GoogleCloud;
using APIConnector.Model;
using AuxiliaryClassLibrary.Network;
using CloudSharpSystemsCoreLibrary.Sessions;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson;
using Z.EntityFramework.Extensions.Internal;

namespace CloudSharpSystemsWeb.Controllers
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

        /*
        [HttpPost("init")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> Init()
        {
            var rand = new Random();
            int part1 = rand.Next(255);
            int part2 = rand.Next(255);
            int part3 = rand.Next(255);
            int part4 = rand.Next(255);
            int rand_size = rand.Next(1000);
            //return await ILoadBalancer.Balance(this._app_db_main_context, this._SITE_ID, $"{part1}.{part2}.{part3}.{part4}", "1", 125);
            return new { Site_ID = _SITE_ID };
        }
        */

        [HttpGet("get_server_load_distribution")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetServerLoadDistributionData()
        {
            List<T_SERVER_LOAD_DISTRIBUTION> distribution_lst = await NetworkLoadBalancingDataContext.GetServerLoadDistributionFunction(this._app_db_main_context, this._SITE_ID);

            var return_lst = distribution_lst.ConvertAll(dist => new
            {
                //site_ID = dist.SITE_ID, 
                host_name = dist.SERIAL_NO,
                server_host_IP = dist.HOST_IP,
                server_host_port = dist.PORT,
                server_host_status = dist.IP_STATUS,
                server_capacity = dist.NET_LOAD_CAPACITY,
                server_location = dist.LOCATION_CODE,
                connection_count = dist.SESSION_COUNT,
                current_load = dist.RESOURCE_LOAD
            }).OrderBy(dist => dist.server_location).ThenBy(dist => dist.server_host_IP);

            return return_lst;
        }

        [HttpGet("get_ip_test")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public string GetIPTest()
        {
            return this.MY_PUBLIC_IP;
        }


        [HttpGet("get_queryable_fields_operations")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public QueryableOperations GetQueryableFieldsOperations(string queryID)
        {
            var queryable_operations = this._app_db_main_context.Validator.GetQueryableOperations(queryID);
            return queryable_operations;
        }


        // GetQueryableOperations
        /*
        protected FieldQueryConfig[] GetQueryableFieldsConfig(IConfigurationSection queryable_fields_config_section, string schema_name, string function_name)
        {
            //bool main_config_exists = queryable_fields_config.Exists();

            var schema_config = queryable_fields_config_section.GetRequiredSection(schema_name);
            //bool schema_config_exists = schema_config.Exists();

            var function_config = schema_config.GetRequiredSection(function_name);
            //bool function_config_exists = function_config.Exists();
            FieldQueryConfig[] queryable_fields_config = function_config.Get<FieldQueryConfig[]>();
            return queryable_fields_config;
        }
        */






        private static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (StringSegment.IsNullOrEmpty(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }
            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
            }
            return boundary.ToString();
        }

        protected async Task ParseFormData(IDictionary<string, Func<MultipartSection?, Task?>> parse_actions) {
            // make sure we have the correct header type
            if (!MediaTypeHeaderValue.TryParse(Request.ContentType, out MediaTypeHeaderValue? contentType)
                || !contentType.MediaType!.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadHttpRequestException("Incorrect mime-type");
            }
            
            // Get the multipart/form-boundary header from the content-type
            // Content-Type: multipart/form-data; boundary="--73dc24e0-b350-48f8-931e-eab338df00e1"
            // The spec says 70 characters is a reasonable limit.
            string boundary = GetBoundary(contentType!, lengthLimit: 70);
            var multipartReader = new MultipartReader(boundary, Request.Body);

            // Use the multipart reader to read each of the sections
            while (await multipartReader.ReadNextSectionAsync() is { } section)
            {
                // Make sure we have a content-type for the section
                if (!MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue? sectionType))
                {
                    throw new BadHttpRequestException("Invalid content type in section " + section.ContentType);
                }

                bool valid_content_type = false;
                foreach (KeyValuePair<string, Func<MultipartSection?, Task?>> parse_action in parse_actions) {
                    if (sectionType!.MediaType!.Value!.Contains(parse_action.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        await parse_action.Value!(section)!;
                        valid_content_type = true;
                    }
                }
                
                if (!valid_content_type)
                {
                    throw new BadHttpRequestException("Invalid content type in section " + section.ContentType);
                }
            }
        }
        

    }
}
