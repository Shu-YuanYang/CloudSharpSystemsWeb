using DBConnectionLibrary.Models;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using DBConnectionLibrary.DBObjectContexts;
using APIConnector.Model;
using Microsoft.Extensions.Options;
using DBConnectionLibrary.DBObjectContexts.Mongo;
using DBConnectionLibrary.DBQueryContexts;

namespace CloudSharpSystemsWeb.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class AppUsercontroller : TemplateController
    {
        public AppUsercontroller(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {

        }

        [HttpPost("query_app_user_activities")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<TB_CENTRAL_SYSTEM_LOG>> QueryAppUserActivities([FromBody] QueryList? query_lst)
        {
            var session_info = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

            // TODO: Implement role system to check authorization:
            if (session_info.THREAD_ID != "20240511173746_8374F1CA-6640-454E-8E8F-192FAACA8B32") throw new UnauthorizedAccessException("Not authorized to view app user activities");

            var posts = await AppUserContext.GetAppUserActivitiesByQuery(this._app_db_main_context, query_lst);
            return posts;
        }

    }
}
