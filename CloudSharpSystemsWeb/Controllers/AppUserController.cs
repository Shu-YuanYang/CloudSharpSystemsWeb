using DBConnectionLibrary.Models;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using DBConnectionLibrary.DBObjectContexts;
using APIConnector.Model;
using Microsoft.Extensions.Options;
using DBConnectionLibrary.DBObjectContexts.Mongo;
using DBConnectionLibrary.DBQueryContexts;
using APIConnector.GoogleCloud;
using System.Security.Authentication;

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



        [HttpGet("get_identity_user_profile")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<T_APP_IDENTITY_USER_PROFILE_HEADER> GetGCPIdentityUserProfile()
        {
            var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);

            // query for aliased profile:
            T_APP_IDENTITY_USER_PROFILE_HEADER profile_header = await AppUserContext.GetUserIdentityProfileHeader(this._app_db_main_context, GCPCredentialsHelper.IDENTITY_PROVIDER, session.THREAD_ID!);

            // fetch identity data from Google:
            GoogleAPIOAuth2UserInfo user_info_data;
            try
            {
                user_info_data = await GoogleAPIHelper.GetUserInfo(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_userinfo")!, session.SESSION_ITEMS!.First().ITEM_POLICY!);
            }
            catch (Exception ex)
            {
                throw new InvalidCredentialException(ex.Message);
            }

            profile_header.FIRST_NAME = ""; // TO REWRITE AFTER PUBLIC TESTING
            profile_header.LAST_NAME = ""; // TO REWRITE AFTER PUBLIC TESTING
            profile_header.PHONE_NUMBER = ""; // TO REWRITE AFTER PUBLIC TESTING
            profile_header.PROFILE_PICTURE = user_info_data.picture;

            return profile_header;
        }




        [HttpGet("get_identity_users_by_team")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<IEnumerable<T_APP_IDENTITY_USER_PROFILE_HEADER>> GetIdentityUserProfilesByTeam(string app_id, string encoded_team_name)
        {
            var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);
            var team_name = Uri.UnescapeDataString(encoded_team_name);

            var teams = await AppTeamContext.GetTeamsByUser(this._app_db_main_context, app_id, session.THREAD_ID!, true);
            var target_teams = teams.Where(team => team.TEAM_NAME == team_name);
            if (!target_teams.Any(team => team.TEAM_NAME == team_name)) {
                return new List<T_APP_IDENTITY_USER_PROFILE_HEADER>();
            }

            var target_team = target_teams.First();

            IEnumerable<T_APP_IDENTITY_USER_PROFILE_HEADER> user_profile_header_lst;

            // query for aliased profiles:
            if (target_team.TEAM_NAME == "My Notes") user_profile_header_lst = new List<T_APP_IDENTITY_USER_PROFILE_HEADER> { await AppUserContext.GetUserIdentityProfileHeader(this._app_db_main_context, GCPCredentialsHelper.IDENTITY_PROVIDER, session.THREAD_ID!) };
            else user_profile_header_lst = await AppUserContext.GetUserIdentityProfileHeadersByTeam(this._app_db_main_context, GCPCredentialsHelper.IDENTITY_PROVIDER, target_team.TEAM_ID!);

            user_profile_header_lst = user_profile_header_lst.Select(profile_header => {
                profile_header.FIRST_NAME = ""; // TO REWRITE AFTER PUBLIC TESTING
                profile_header.LAST_NAME = ""; // TO REWRITE AFTER PUBLIC TESTING
                profile_header.PHONE_NUMBER = ""; // TO REWRITE AFTER PUBLIC TESTING
                //profile_header.PROFILE_PICTURE; // TODO; To fetch from GCP storage.
                return profile_header;
            });
            
            return user_profile_header_lst;
        }

    }
}
