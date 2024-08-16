using Amazon.Lambda.Core;
using APIConnector.IPHelper;
using APIConnector.Model;
using AuxiliaryClassLibrary.DateTimeHelper;
using AuxiliaryClassLibrary.Network;
using CloudSharpLimitedCentralWeb.Models;
using CloudSharpSystemsCoreLibrary.Models;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace CloudSharpSystemsWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : TemplateController
    {

        private static long available_capacity = 2300;
        private static int resource_unit_counter = 10000;
        private static float preset_error_rate = 0.0f; 

        public TestController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {

        }



        private bool __get_simulated_random_error_flag() 
        {
            int scale = 1000;
            Random gen = new Random();
            int prob = gen.Next(scale);
            return prob < preset_error_rate * scale;
        }

        [HttpGet("test_get_monitor_signal")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public Object TestGetMonitorSignal()
        {
            bool error_flag = this.__get_simulated_random_error_flag();
            if (error_flag)
                throw new Exception("Unknown server error occurred (simulated)!");

            return new { probe_status = "OK" };
        }



        private async Task<LoadedConnectionStartSessionResponse> start_connection(TestClientLoadedConnectionData client_data) 
        {
            // Probabilistically throw error based on TestController.preset_error_rate
            bool error_flag = this.__get_simulated_random_error_flag();
            if (error_flag)
                throw new Exception("Unknown server error occurred (simulated)!");


            // Start connection:
            DateTime start_time = DateTime.Now;

            string? proxy_client_IP;
            bool any_proxy_client_IP = HttpRequestHeaderHelper.TryParseHeaderValueFirst(Request, ReverseProxyHttpHeader.Client_IP, out proxy_client_IP);
            string? proxy_thread_ID;
            bool any_proxy_thread_ID = HttpRequestHeaderHelper.TryParseHeaderValueFirst(Request, ReverseProxyHttpHeader.Trace_Identifier, out proxy_thread_ID);
            string? proxy_requested_time;
            bool any_proxy_requested_time = HttpRequestHeaderHelper.TryParseHeaderValueFirst(Request, ReverseProxyHttpHeader.Requested_Time, out proxy_requested_time);


            //string client_IP = (String.IsNullOrEmpty(test_IP))? HttpContext.Connection.RemoteIpAddress!.ToString() : test_IP;
            string client_IP = (any_proxy_client_IP) ? proxy_client_IP! : Request.HttpContext.Connection.RemoteIpAddress!.ToString();
            long message_resource_size = HttpRequestHeaderHelper.ASSUMED_HEADER_SIZE + (HttpContext.Request.ContentLength ?? 0) ;
            //var httpRequestTimeFeature = HttpContext.Features.Get<IHttpRequestTimeFeature>();
            string trace_ID = (any_proxy_thread_ID) ? proxy_thread_ID! : HttpContext.TraceIdentifier;

            // Find client geolocation:
            var ClientIPGeoInfo = await LocalIP.GetIPGeoLocation(client_IP);
            string client_location = client_data.location + $" ({ClientIPGeoInfo.city}, {ClientIPGeoInfo.region}, [{ClientIPGeoInfo.lat}, {ClientIPGeoInfo.lon}])";

            TB_USER_SESSION session_data = new TB_USER_SESSION
            {
                CLIENT_IP = client_IP,
                THREAD_ID = trace_ID,
                CLIENT_LOCATION = client_location,
                //EDIT_TIME = DateTime.Now,
                HOST_IP = this.MY_PUBLIC_IP,
                RESOURCE_SIZE = (int)message_resource_size,
                IS_VALID = 'Y',
                EDIT_BY = "CloudSharpSystems",
                RESOURCE_UNIT = client_data.resource_unit,
                SESSION_ITEMS = client_data.request_items?.ConvertAll(item => new TB_USER_SESSION_ITEM
                {
                    ITEM_NAME = item.item_name,
                    ITEM_DESCRIPTION = item.item_description,
                    ITEM_SIZE = (int) item.item_size,
                    EDIT_BY = client_IP
                })
            };


            DateTime end_time;
            double delay;
            var response_obj = new LoadedConnectionStartSessionResponse
            {
                client_ip = session_data.CLIENT_IP,
                thread_id = session_data.THREAD_ID,
                client_location = session_data.CLIENT_LOCATION,

                server_host_ip = session_data.HOST_IP,
                server_location = "",
            };

            

            // deduct resource: Shu-Yuan Yang 11062023 moved before DB transaction to act as first overload responder.
            if (TestController.available_capacity >= 0) TestController.available_capacity -= session_data.RESOURCE_SIZE;
            if (TestController.available_capacity < 0)
            {
                response_obj.connection_data = new LoadedConnectionStartSessionConnectionData
                {
                    connection_status = "failure",
                    resource_unit = -1,
                    resource_size = (int)message_resource_size,
                    resource_request_response = "resource denied"
                };
                response_obj.message = "Resource denied. Server has reached its maximum capacity";

                end_time = DateTime.Now;
                delay = (end_time - start_time).TotalMilliseconds;
                response_obj.processing_delay = delay;

                return response_obj;
            }


            var session_obj = session_data;
            TB_SERVER server_info = new TB_SERVER { };
            await DBTransactionContext.DBTransact(this._app_db_main_context, async (context, transaction) =>
            {
                //await NetworkUserSessionContext.LockUserSessionTable(context);
                DateTime current_time = await DBTransactionContext.DBGetDateTime(context);
                session_data.REQUESTED_TIME = (any_proxy_requested_time) ? TimestampHelper.ToPreciseFormatDateTime(proxy_requested_time!) : current_time;
                //session_data.EDIT_TIME = current_time;
                //session_data.SESSION_ID = session_data.REQUESTED_TIME.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("D");
                session_data.SESSION_ID = TimestampHelper.ToTimeStampIDFormatString(session_data.REQUESTED_TIME) + "_" + Guid.NewGuid().ToString("D");
                session_data.EDIT_BY = this.APP_ID;
                session_data = await NetworkUserSessionContext.InsertNewUserSession(context, session_data);
                server_info = await ProductsServerContext.GetServerInfoBySiteAndIP(context, this.SITE_ID, this.MY_PUBLIC_IP);
            });

            IPGeoLocationObject HostIPGeoInfo = JsonSerializer.Deserialize<IPGeoLocationObject>(server_info.RACK_CODE!)!;

            response_obj.client_requested_time = session_obj.REQUESTED_TIME;
            response_obj.server_location = server_info.LOCATION_CODE + $", [{HostIPGeoInfo.lat}, {HostIPGeoInfo.lon}]";
            response_obj.connection_data = new LoadedConnectionStartSessionConnectionData
            {
                connection_status = "success",
                resource_unit = session_obj.RESOURCE_UNIT,
                resource_size = session_obj.RESOURCE_SIZE,
                resource_request_response = "resource procured"
            };
            response_obj.message = "Connected";

            end_time = DateTime.Now;
            delay = (end_time - start_time).TotalMilliseconds;
            response_obj.processing_delay = delay;

            return response_obj;
        }



        [HttpGet("start_fixed_size_connection")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<LoadedConnectionStartSessionResponse> StartSession()
        {
            TestClientLoadedConnectionData client_data = new TestClientLoadedConnectionData { resource_unit = ++TestController.resource_unit_counter };
            return await this.start_connection(client_data);
        }


        [HttpPost("start_session")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<LoadedConnectionStartSessionResponse> StartSession([FromBody] TestClientLoadedConnectionData client_data)
        {
            //throw new Exception("Surprise!");
            return await this.start_connection(client_data);
        }



        [HttpPost("reset_session_resources")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<TestServerResourceResetConfig> ResetSessionResources([FromBody] TestSessionResourceResetConfig resource_configuration) {

            TestServerResourceResetConfig config_response = new TestServerResourceResetConfig { 
                server_host_ip = this.MY_PUBLIC_IP,
                capacity = -1
            };

            config_response.reset_status = "failure";
            config_response.server_host_message = "No server configuration is given.";
            if (resource_configuration.server_capacities == null) return config_response;
            
            var this_host_queryable = resource_configuration.server_capacities.Where(h => h.server_host_ip == this.MY_PUBLIC_IP);

            if (!this_host_queryable.Any()) return config_response;

            TestServerResourceResetConfig server_config = this_host_queryable.First();

            await DBTransactionContext.DBTransact(this._app_db_main_context, async (context, transaction) =>
            {
                await ProductsServerContext.ResetServerCapacity(context, server_config.server_host_ip!, server_config.capacity, server_config.preset_error_rate, server_config.server_host_ip!);
            });
            
            TestController.available_capacity = server_config.capacity;
            TestController.preset_error_rate = server_config.preset_error_rate;
            TestController.resource_unit_counter = 10000;

            config_response.capacity = server_config.capacity;
            config_response.preset_error_rate = server_config.preset_error_rate;
            config_response.reset_status = "success";
            config_response.server_host_message = "Server capacity updated.";
            return config_response;
        }

        /*
        [HttpGet("test_list_headers")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public Object TestListHeaders() {
            var headers = Request.Headers;
            return headers;
        } 
        */
    }
}
