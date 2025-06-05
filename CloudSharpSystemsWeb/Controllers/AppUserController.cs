using DBConnectionLibrary.Models;
using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using DBConnectionLibrary.DBObjectContexts;
using APIConnector.Model;
using Microsoft.Extensions.Options;
using DBConnectionLibrary.DBQueryContexts;
using APIConnector.GoogleCloud;
using System.Security.Authentication;
using AuxiliaryClassLibrary.Network;
using CloudSharpSystemsCoreLibrary.Models;
using CloudSharpSystemsCoreLibrary.Sessions;
using System.Text.Json;
using CloudSharpSystemsCoreLibrary.Security;
using Microsoft.EntityFrameworkCore;

namespace CloudSharpSystemsWeb.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class AppUsercontroller : TemplateController
    {
		private readonly GCPOAuth2ClientSecretKeyObject _gcp_client_secrets;

		public AppUsercontroller(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor, IOptions<GCPOAuth2ClientSecretKeyObject> GCPOAuth2CredentialsClientSecretAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {
			this._gcp_client_secrets = GCPOAuth2CredentialsClientSecretAccessor.Value;
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
            var session = await this._session_manager.GetLinkedSessionByAuthorizationHeader(Request, GCPCredentialsHelper.IDENTITY_PROVIDER); //await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);

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
            var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");
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



		[HttpGet("identity_refresh_token")]
		[Produces("application/json")]
		[Consumes("application/json")]
		public async Task<TB_USER_SESSION> GCPIdentityRefreshToken()
		{
			//var session = await this._session_manager.GetLinkedSessionByAuthorizationHeader(Request, GCPCredentialsHelper.IDENTITY_PROVIDER); //await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);
			var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");

			var session_item = session.SESSION_ITEMS!.First(i => i.ITEM_NAME!.Contains("IDENTITY/"));
			var token_info = JsonSerializer.Deserialize<GoogleAPIOauth2TokenResponse>(session_item.ITEM_DESCRIPTION!);
			string refresh_token = token_info!.refresh_token!;
            GoogleAPIOAuth2UserInfo user_info;

            if (session_item.ITEM_ROUTE == GCPCredentialsHelper.IDENTITY_PROVIDER)
            {
                // Revoke access token and refresh token: Doc at https://developers.google.com/identity/protocols/oauth2/web-server#httprest_8 - Revoking a Token
                string refresh_result = await this._gcp_credentials_helper.RefreshOAuth2AccessToken(this._gcp_client_secrets, refresh_token);
                // parse new token info:
                token_info = JsonSerializer.Deserialize<GoogleAPIOauth2TokenResponse>(refresh_result);
                token_info!.issued_utc = DateTime.UtcNow;
                token_info!.refresh_token = refresh_token;
				user_info = await GoogleAPIHelper.GetUserInfo(_external_api_map.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_userinfo")!, token_info.access_token!);
			}
			else if (session_item.ITEM_ROUTE == SessionManager.CLOUDSHARP_IDENTITY_PROVIDER)
            {
                token_info!.access_token = SecurityStateGenerator.GenerateAuthenticationToken();
                token_info!.expires_in = 3599;
                token_info!.token_type = "Bearer";
                token_info!.id_token = session.THREAD_ID;
                token_info!.refresh_token = refresh_token;
                token_info!.issued_utc = DateTime.UtcNow;
                TB_APP_USER_IDENTITY identity = await this._session_manager.GetUserIdentity(GCPCredentialsHelper.IDENTITY_PROVIDER, null, session.THREAD_ID);
				user_info = new GoogleAPIOAuth2UserInfo
				{
					email = identity.USERNAME,
					id = identity.USERID,
					verified_email = true
				};
            }
            else throw new NotImplementedException($"Token refresh for identity provider {session_item.ITEM_ROUTE} has not been implemented!");

			// update session
			var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(Request.HttpContext);
			SessionManager session_manager = new SessionManager(this._app_db_main_context);
			var session_data = await session_manager.UpdateSession(client_info, token_info, user_info, this.APP_ID, "TOKEN_REFRESHED");

			return new TB_USER_SESSION
			{
				SESSION_ID = session_data.SESSION_ID,
				CLIENT_IP = session_data.CLIENT_IP,
				HOST_IP = session_data.HOST_IP,
				RESOURCE_UNIT = session_data.RESOURCE_UNIT,
				CLIENT_LOCATION = session_data.CLIENT_LOCATION,
				REQUESTED_TIME = session_data.REQUESTED_TIME,
				RESOURCE_SIZE = session_data.RESOURCE_SIZE
			};
		}


		[HttpGet("identity_log_out")]
		[Produces("application/json")]
		[Consumes("application/json")]
		public async Task<GeneralAPIResponse> GCPIdentityLogOut()
		{
			//var session = await this._session_manager.GetLinkedSessionByAuthorizationHeader(Request, GCPCredentialsHelper.IDENTITY_PROVIDER);
			var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, false, "");
			var session_item = session.SESSION_ITEMS!.First(i => i.ITEM_NAME!.Contains("IDENTITY/"));

            string logout_result;
			if (session_item.ITEM_ROUTE == GCPCredentialsHelper.IDENTITY_PROVIDER)
            {
                logout_result = await this._gcp_credentials_helper.RevokeOAuth2AccessToken(session_item.ITEM_POLICY!);
            }
            else 
            {
                logout_result = "CloudSharp logout success.";
            }

			// Write system log to record token revoking:
			var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(Request.HttpContext);
			await this._session_manager.WriteLogOutSystemLog(client_info, session, session_item, this.APP_ID, logout_result);

            // Session deletion transaction:
            List<TB_USER_SESSION> sessions_to_delete = new List<TB_USER_SESSION> { session };
			if (session_item.ITEM_ROUTE != SessionManager.CLOUDSHARP_IDENTITY_PROVIDER)
			{
                var active_sessions = await NetworkUserSessionContext.GetUserSessions(this._app_db_main_context, new TB_USER_SESSION { THREAD_ID = session.THREAD_ID, IS_VALID = 'Y' });
                active_sessions = active_sessions.Where(s => s.SESSION_ITEMS?.Any(i => i.ITEM_NAME == $"IDENTITY/{SessionManager.CLOUDSHARP_IDENTITY_PROVIDER}") ?? false);
                sessions_to_delete.AddRange(active_sessions);
            }
			await this._session_manager.InvalidateSessions(sessions_to_delete);

			return new GeneralAPIResponse { Status = "OK", Message = "Logged out." };
		}

	}
}
