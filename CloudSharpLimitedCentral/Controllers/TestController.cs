using APIConnector;
using APIConnector.Model;
using AuxiliaryClassLibrary.DateTimeHelper;
using AuxiliaryClassLibrary.Network;
using Azure;
using Azure.Core;
using CloudSharpLimitedCentralWeb.Models;
using CloudSharpSystemsCoreLibrary.Models;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace CloudSharpLimitedCentral.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public const string SITE_CONFIG_KEY = "SiteConfig";


        protected readonly ILogger<TestController> _logger;
        protected readonly AppDBMainContext _app_db_main_context;
        protected readonly IConfiguration _configuration;

        private readonly string _SITE_ID;

        public TestController(ILogger<TestController> logger, IConfiguration config, AppDBMainContext appDBMainContext)
        {
            _logger = logger;
            _app_db_main_context = appDBMainContext;
            _configuration = config;

            _SITE_ID = config[TestController.SITE_CONFIG_KEY + ":" + this.GetType().Name]!;
        }





        [HttpPost("reset_session_resources")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> ResetSessionResources([FromBody] TestSessionResourceResetConfig resource_configuration) {

            

            // 1. Reset server capacities:
            var hostList = await NetworkWebsiteHostContext.GetWebsiteHostsBySiteID(_app_db_main_context, this._SITE_ID);
            List<TestServerResourceResetConfig> responses = new List<TestServerResourceResetConfig>();
            //List<Task<string> > response_tasks = new List<Task<string>>();
            List<string> response_str_array = new List<string>();
            foreach (var host in hostList)
            {
                string target_url_str = APIConnector.RESTAPIConnector.ConstructRESTBaseURL(host.HOST_IP!);
                target_url_str = target_url_str.Remove(target_url_str.Length - 1, 1);
                target_url_str = target_url_str + Request.Path.Value!;

                var response_task = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
                {
                    URL = target_url_str,
                    ContentType = "application/json",
                    APIType = RESTAPIType.POST,
                    Parameters = "",
                    InputObject = resource_configuration
                });
                response_str_array.Add(response_task);
                //response_tasks.Add(response_task);
            }
            //var response_str_array = await Task.WhenAll(response_tasks);


            // 2. Parse each response:
            for (int i = 0; i < response_str_array.Count(); ++i)
            {
                string response_message = response_str_array[i];

                TestServerResourceResetConfig response;
                try
                {
                    response = JsonSerializer.Deserialize<TestServerResourceResetConfig>(response_message)!;
                    responses.Add(response);
                }
                catch
                {
                    //response = new TestServerResourceResetConfig { server_host_ip = host.HOST_IP, capacity = -1, reset_status = "failure", server_host_message = "Unable to reach host." };
                }
            }


            // 3. Add message for unreacheable hosts:
            var problem_hosts = hostList.Where(h => !responses.Any(r => r.server_host_ip == h.HOST_IP));
            foreach (var problem_host in problem_hosts)
            {
                var response = new TestServerResourceResetConfig { server_host_ip = problem_host.HOST_IP, capacity = -1, reset_status = "failure", server_host_message = "Unable to reach host." };
                responses.Add(response);
            }


            // Transaction:
            // 4. Reset load balancer cache, reset load balancing algorithm
            await NetworkLoadBalancingDataContext.LoadBalanceResetProcedure(_app_db_main_context, this._SITE_ID, resource_configuration.load_balancing_algorithm!, resource_configuration.load_balancing_max_search_count, "CloudSharpSystemsWeb");



            // 5. Construct status object:
            var reset_response_obj = new
            {
                resource_configuration.load_balancing_algorithm,
                resource_configuration.load_balancing_max_search_count,
                resource_configuration.request_batch_size,
                server_reset_status = responses,
                message = (responses.Any(r => r.reset_status == "failure"))? "One or more settings encountered reset errors." : "Server settings reset complete."
            };

            return reset_response_obj;
        }

    }
}
