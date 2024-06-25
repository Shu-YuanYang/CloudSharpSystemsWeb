using CloudSharpLimitedCentralWeb.Models;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CloudSharpLimitedCentral.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SystemErrorController : ControllerBase
    {
        

        protected readonly ILogger<WeatherForecastController> _logger;
        protected readonly AppDBMainContext _app_db_main_context;
        protected readonly IConfiguration _configuration;
        private readonly string _MY_PUBLIC_IP;

        public SystemErrorController(ILogger<WeatherForecastController> logger, IConfiguration config, AppDBMainContext appDBMainContext)
        {
            _logger = logger;
            _app_db_main_context = appDBMainContext;
            _configuration = config;
            _MY_PUBLIC_IP = APIConnector.IPHelper.LocalIP.GetMyIP().Result!;
        }


        [HttpGet("resource_unavailable")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public LoadedConnectionStartSessionResponse GetResourceUnavailableMessage()
        {
            string ClientIP = HttpContext.Connection.RemoteIpAddress!.ToString();
            string TraceID = HttpContext.TraceIdentifier;
            int ResourceSize = (int) Request.ContentLength!;

            return new LoadedConnectionStartSessionResponse
            {
                client_ip = ClientIP,
                thread_id = TraceID,
                client_location = "",
                client_requested_time = DateTime.Now,
                server_host_ip = null,
                server_location = null,
                connection_data = new LoadedConnectionStartSessionConnectionData
                {
                    connection_status = "failure",
                    resource_unit = -1,
                    resource_size = ResourceSize,
                    resource_request_response = "Server resource unavailable."
                },
                processing_delay = 0,
                message = "Server resource unavailable."
            };
        }


        [HttpGet("caught_server_error/{ip}")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public LoadedConnectionStartSessionResponse GetCaughtServerError(string ip)
        {
            string ClientIP = HttpContext.Connection.RemoteIpAddress!.ToString();
            string TraceID = HttpContext.TraceIdentifier;
            int ResourceSize = (int)Request.ContentLength!;

            return new LoadedConnectionStartSessionResponse
            {
                client_ip = ClientIP,
                thread_id = TraceID,
                client_location = "",
                client_requested_time = DateTime.Now,
                server_host_ip = ip,
                server_location = null,
                connection_data = new LoadedConnectionStartSessionConnectionData
                {
                    connection_status = "failure",
                    resource_unit = -1,
                    resource_size = ResourceSize,
                    resource_request_response = "Error."
                },
                processing_delay = 0,
                message = "Server responded with an error."
            };
        }


    }
}
