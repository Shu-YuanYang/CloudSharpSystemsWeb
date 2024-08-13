using APIConnector.Model;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CloudSharpSystemsWeb.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NetworkMonitorController : TemplateController
    {
        public NetworkMonitorController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {

        }

        [HttpGet("get_task_status_statistics")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetRecentHostStatusLogs(string app_id)
        {
            var table_result = await AppDataContext.GetProgramStatuses(this._app_db_main_context, new TB_PROGRAM_STATUS { APP_ID = app_id, PROGRAM_TYPE = "TASK" });

            var grouped_statistics = table_result
                .GroupBy(row => row.PROGRAM_STATUS)
                .OrderBy(g => g.Key)
                .Select(g => new { status = g.Key, statistics = g.ToList() });

            return grouped_statistics;
        }




    }
}
