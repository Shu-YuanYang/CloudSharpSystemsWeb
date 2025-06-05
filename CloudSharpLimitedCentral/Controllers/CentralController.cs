
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using CloudSharpSystemsCoreLibrary.Messaging;
using CloudSharpSystemsCoreLibrary.Models;
using CloudSharpSystemsCoreLibrary.Security;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.DBObjectContexts.Mongo;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

namespace CloudSharpLimitedCentral.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CentralController : ControllerBase
    {
        public const string SITE_CONFIG_KEY = "SiteConfig";


        protected readonly ILogger<TestController> _logger;
        protected readonly AppDBMainContext _app_db_main_context;
        protected readonly AppDBMongoContext _app_db_mongo_context;
        protected readonly IConfiguration _configuration;

        private readonly string _SITE_ID;

        public CentralController(ILogger<TestController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext)
        {
            _logger = logger;
            _app_db_main_context = appDBMainContext;
            _app_db_mongo_context = appDBMongoContext;
            _configuration = config;
            _SITE_ID = config[TestController.SITE_CONFIG_KEY + ":" + this.GetType().Name]!;
        }





        [HttpPost("update_task_statuses")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> UpdateTaskStatuses(string app_id, string program_name/*, [FromBody] TestSessionResourceResetConfig resource_configuration*/) {

            string severity = "GOOD";
            string program_type = "TASK";

            await DBTransactionContext.DBTransact(this._app_db_main_context, async (context, transaction) =>
            {
                var log = await AppDataContext.WriteSystemLog(context, new TB_CENTRAL_SYSTEM_LOG { 
                    APP_ID = app_id,
                    SYSTEM_NAME = this._configuration["HostConfig"]!,
                    TRACE_ID = "(UNAVAILABLE)",
                    RECORD_TYPE = severity,
                    RECORD_KEY = program_type,
                    RECORD_VALUE1 = $"program: {program_name}",
                    RECORD_VALUE2 = $"logName: projects/{app_id}/logs/{program_name}",
                    RECORD_VALUE3 = "",
                    RECORD_VALUE4 = $"severity: {severity}",
                    RECORD_VALUE5 = "details: see Google Cloud Logging (find latest log by logName in column RECORD_VALUE2)",
                    RECORD_MESSAGE = "message: see Google Cloud Logging (find latest log by logName in column RECORD_VALUE2)",
                    RECORD_NOTE = "Cloud Scheduler job successfully triggered.",
                    EDIT_BY = program_name
                });

                var status_record = await AppDataContext.UpdateProgramStatus(context, new TB_PROGRAM_STATUS {
                    PROGRAM_ID = program_name,
                    APP_ID = app_id,
                    LAST_TRACE_ID = "(UNAVAILABLE)",
                    PROGRAM_STATUS = severity,
                    LAST_LOG_TIME = log.EDIT_TIME,
                    NOTES = log.RECORD_NOTE,
                    EDIT_BY = program_name
                });

                await AppDataContext.UpdateTaskStatuses(context, app_id, program_type, program_name);
            });

            return new GeneralAPIResponse { Status = "Success", Message = "Task statuses updated!" }; ;
        }


		[HttpGet("get_requester_domain")]
		[Produces("application/json")]
		[Consumes("application/json")]
		public object GetRequesterDomain()
		{
			string clientDomain = HttpContext.Request.Headers.Origin.ToString();
            var uriBuilder = new UriBuilder(clientDomain);

			return new { RequesterDomain = clientDomain, RequesterHost = uriBuilder.Host };
			//string ClientIP = HttpContext.Connection.RemoteIpAddress!.ToString();
			//string TraceID = HttpContext.TraceIdentifier;
			//int ResourceSize = (int)Request.ContentLength!;

		}


        [HttpGet("cloudsharp_login")]
        public async Task<IActionResult> CloudSharpLoginView(string verification_method, string message_id) {
            var sent_emails = await EmailContext.GetTemplatedEmailsByIDs(this._app_db_mongo_context, new string[] { message_id });
            var email = sent_emails.Single();

			string file_path = "Views/access_code_verification.html"; //Path.GetFullPath((new Uri("../../../../EmailDebutTemplate.cshtml")).LocalPath);
		    string content = System.IO.File.ReadAllText(file_path);
			content = HTMLGenerator.GetHTMLFromTemplate(content, new object[] { email.recipients!.First() });

			return Content(content, "text/html");
		}



        public class __VerificationInput {
            public string? verification_method { get; set; }
            public string? message_id { get; set; }
            public string? verification_code { get; set; }
            public string? state { get; set; }
        }

        [HttpPost("cloudsharp_submit_login")]
        [Consumes("application/json")]
        public async Task<object> CloudSharpSubmitLogin([FromBody] __VerificationInput input) {

            input.verification_method = input.verification_method!.ToUpper();
			if (input.verification_method != "EMAIL") throw new InvalidOperationException($"Invalid verification method: {input.verification_method}");
            input.state = Uri.UnescapeDataString(input.state!);

			var templated_emails = await EmailContext.GetTemplatedEmailsByIDs(this._app_db_mongo_context, new string[] { input.message_id! });
            var authentication_email = templated_emails.Single();
            // Verify code, state, and expiration time:
            dynamic metadata = authentication_email.metadata!;
            string verification_code = metadata.code;
            string state = metadata.state;
            DateTime expiration_time = metadata.expiration_time;
            expiration_time = expiration_time.ToUniversalTime();

            if (input.verification_code != verification_code) throw new AuthenticationException("Incorrect login code!");
            if (input.state != state) throw new AuthenticationException("Incorrect state!");
            if (expiration_time < DateTime.UtcNow) throw new AuthenticationException("Code has expired!");

            string authorization_code = SecurityStateGenerator.GenerateAuthorizationCode(32);

            bool is_auth_updated = await EmailContext.UpdateTemplatedEmail(this._app_db_mongo_context, new CL_TEMPLATED_EMAIL { 
                _id = authentication_email._id,
                metadata = new {
					state = state,
					code = verification_code,
					expiration_time = expiration_time,
                    auth_code = authorization_code
				}
            });

            if (!is_auth_updated) throw new AuthenticationException("An unknown failure occurred during user authentication!");

            return new { response_url = $"/auth/cloudsharp/authenticate?code={authorization_code}&state={Uri.EscapeDataString(state)}" };
        }

	}
}
