using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using DBConnectionLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Data.SqlClient;
using DBConnectionLibrary.DBObjectContexts;
using CloudSharpLimitedCentralWeb.Models;
using Microsoft.Extensions.Options;
using APIConnector.Model;

namespace CloudSharpSystemsWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SupplyChainController : TemplateController
    {

        public SupplyChainController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
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
            return ILoadBalancer.Balance("SUPPLY_CHAIN_PAGE", $"{part1}.{part2}.{part3}.{part4}", "1", rand_size);
        }
        */

        [HttpGet("get_test_data")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetTestData()
        {
            /*
            DBTransactionContext.DBTransact(_app_db_main_context, (DBContext) => {
                AppDataContext.GetAllAppData(DBContext);
            });
            */
            var apps = await AppDataContext.GetAllAppData(_app_db_main_context);
            return apps;
        }


        

    }
}
