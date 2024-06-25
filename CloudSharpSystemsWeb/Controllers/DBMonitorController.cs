using DBConnectionLibrary.Models;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using DBConnectionLibrary.DBObjectContexts;
using APIConnector.Model;
using Microsoft.Extensions.Options;
using DBConnectionLibrary.DBObjectContexts.Mongo;

namespace CloudSharpSystemsWeb.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class DBMonitorController : TemplateController
    {
        public DBMonitorController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {

        }

        [HttpGet("get_recent_host_status_logs")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<T_HOST_LATENCY_STATISTICS>> GetRecentHostStatusLogs(int offset_time_in_hours)
        {
            TB_WEBSITE_HOST app_db_host = (await NetworkWebsiteHostContext.GetWebsiteHostsBySiteID(this._app_db_main_context, this.SITE_ID)).First();
            var result_lst = await NetworkWebsiteHostContext.GetRecentHostLatencyStatistics(this._app_db_main_context, offset_time_in_hours, 15, new string[] { app_db_host.HOST_IP! });
            return result_lst.First();
        }

        [HttpGet("get_recent_db_status_logs")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetRecentDBStatusLogs(int offset_time_in_hours)
        {
            var app_db_hosts = await NetworkWebsiteHostContext.GetWebsiteHostsBySiteID(this._app_db_main_context, this.SITE_ID);
            var host_arr = app_db_hosts.Select(host => host.HOST_IP!).ToArray();
            var result_lst = await NetworkWebsiteHostContext.GetRecentHostLatencyStatistics(this._app_db_main_context, offset_time_in_hours, 15, host_arr);

            var indices = Enumerable.Range(0, host_arr.Length);
            var results = indices.Select(index => {
                return new
                {
                    host = host_arr[index],
                    latency_statistics = result_lst[index]
                };
            });

            return results;
        }

        [HttpGet("test_clear_old_notes")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task TestClearOldNotes() {
            await TeamNoteContext.ClearOldTeamNotes(this._app_db_mongo_context, 24, this.APP_ID);
        }
    }
}
