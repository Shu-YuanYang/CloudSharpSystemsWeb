using APIConnector.Model;
using APIConnector;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using AuxiliaryClassLibrary.Network;
using DBConnectionLibrary.Models;
using Microsoft.Extensions.Options;

namespace CloudSharpSystemsWeb.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class HomeController : TemplateController
    {
        
        public HomeController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {
            
        }

        /*
        [HttpPost("init")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public Object Init()
        {
            var rand = new Random();
            int part1 = rand.Next(1000);
            int part2 = rand.Next(1000);
            int part3 = rand.Next(1000);
            int part4 = rand.Next(1000);
            int rand_size = rand.Next(1000);
            return ILoadBalancer.Balance("HOME_PAGE", $"{part1}.{part2}.{part3}.{part4}", "1", rand_size);
        }
        */

        [HttpGet("get_test_proc_data")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetTestProcData()
        {
            string id = await AppDataContext.GetTestProcedureOutput(_app_db_main_context);
            return id;
        }


        [HttpGet("get_google_data")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetGoogleData()
        {
            var response = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = "https://www.google.com/",
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = ""
            });

            return response;
        }


        [HttpGet("get_host_header")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public Object GetHostHeader() {
            
            return new { header_header = Request.Host.Host };
        }


        [HttpGet("get_system_log_volume")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<T_CENTRAL_SYSTEM_LOG_VOLUME>> GetSystemLogVolume()
        {
            return await AppDataContext.GetRecentSystemLogVolume(this._app_db_main_context, 14, this.APP_ID);
        }

        //GetRecentSystemLogVolume
    }
}
