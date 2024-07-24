using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using APIConnector.Model;
using APIConnector.GoogleCloud;
using AuxiliaryClassLibrary.Network;
using DBConnectionLibrary.Models;
using DBConnectionLibrary.DBObjectContexts;
using CloudSharpSystemsCoreLibrary.Models;
using System.Security.Authentication;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CloudSharpSystemsCoreLibrary.Sessions;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Extensions.Hosting;
using CloudSharpLimitedCentralWeb.Models;
using DBConnectionLibrary.DBObjectContexts.Mongo;
using DBConnectionLibrary.Models.Mongo;

namespace CloudSharpSystemsWeb.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class GCPAssetController : TemplateController
    {
        private readonly GCPOAuth2ClientSecretKeyObject _gcp_client_secrets;

        public GCPAssetController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor, IOptions<GCPOAuth2ClientSecretKeyObject> GCPOAuth2CredentialsClientSecretAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {
            this._gcp_client_secrets = GCPOAuth2CredentialsClientSecretAccessor.Value;
        }

        [HttpGet("get_storage_object_url")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetGCPStorageObjectURL(string bucket, string directory, string object_name)
        {
            var urlSigner = GCPCredentialsHelper.GetURLSigner(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj);
            string signed_url = await GoogleAPIHelper.GenerateV4SignedReadUrl(urlSigner, bucket, $"{directory}/{object_name}"/*"DATA_TABLE_MENU/cs-educator-posts-table.png"*/);
            return new { status = "OK", signed_url = signed_url };
        }



        // TODO: Deprecate this function. Move to AppUserController.cs
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





        [HttpGet("identity_refresh_token")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<TB_USER_SESSION> GCPIdentityRefreshToken()
        {
            var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);
            var GCP_session_item = session.SESSION_ITEMS!.First();

            var token_info = JsonSerializer.Deserialize<GoogleAPIOauth2TokenResponse>(GCP_session_item.ITEM_DESCRIPTION!);
            string refresh_token = token_info!.refresh_token!;

            // Revoke access token and refresh token: Doc at https://developers.google.com/identity/protocols/oauth2/web-server#httprest_8 - Revoking a Token
            string refresh_result = await this._gcp_credentials_helper.RefreshOAuth2AccessToken(this._gcp_client_secrets, refresh_token);

            // parse new token info:
            token_info = JsonSerializer.Deserialize<GoogleAPIOauth2TokenResponse>(refresh_result);
            token_info!.issued_utc = DateTime.UtcNow;
            token_info!.refresh_token = refresh_token;

            // update session
            var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(Request.HttpContext);
            var user_info = await GoogleAPIHelper.GetUserInfo(_external_api_map.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_userinfo")!, token_info.access_token!);
            SessionManager session_manager = new SessionManager(this._app_db_main_context);
            var session_data = await session_manager.UpdateSession(this.MY_PUBLIC_IP, client_info, token_info, user_info, this.APP_ID, "TOKEN_REFRESHED");
            
            return new TB_USER_SESSION { 
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
        public async Task<GeneralAPIResponse> GCPIdentityLogOut() {

            var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);
            var GCP_session_item = session.SESSION_ITEMS!.First();

            
            string logout_result = await this._gcp_credentials_helper.RevokeOAuth2AccessToken(GCP_session_item.ITEM_POLICY!);
            
            // Write system log to record token revoking:
            var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(Request.HttpContext);
            await this._session_manager.WriteLogOutSystemLog(this.MY_PUBLIC_IP, client_info, session, GCP_session_item, this.APP_ID, logout_result);

            // Session deletion transaction:
            await this._session_manager.InvalidateSession(session);

            return new GeneralAPIResponse { Status = "OK", Message = "Logged out." };
        }



        [HttpGet("get_google_daily_trends")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<CL_GOOGLE_DAILY_TREND>> GetGoogleDailyTrends() {
            var results = await GoogleTrendsContext.GetLatestTrendSearches(this._app_db_mongo_context, 2, 4);
            if (1 < results.Count() && results[0].FEED_TIME.Date.Equals(results[1].FEED_TIME.Date)) return new List<CL_GOOGLE_DAILY_TREND>{ results.First() }; // Data from the same day
            return results;
        }

    }

}
