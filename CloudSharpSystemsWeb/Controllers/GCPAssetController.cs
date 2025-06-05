using DBConnectionLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using APIConnector.Model;
using APIConnector.GoogleCloud;
using DBConnectionLibrary.Models;
using DBConnectionLibrary.DBObjectContexts;
using System.Security.Authentication;
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








        [HttpGet("get_google_daily_trends")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<List<CL_GOOGLE_DAILY_TREND>> GetGoogleDailyTrends() {
            var results = await GoogleTrendsContext.GetLatestTrendSearches(this._app_db_mongo_context, 2, 4);
            if (1 < results.Count() && results[0].FEED_TIME.Date.Equals(results[1].FEED_TIME.Date)) return new List<CL_GOOGLE_DAILY_TREND>{ results.First() }; // Data from the same day
            return results;
        }


        [HttpGet("get_gcp_task_logs")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetGCPTaskLogs() {
            var loggingClient = GCPCredentialsHelper.GetLoggingClient(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_logging_read")!, this._gcp_service_account_key_obj);
            var result = await GoogleAPIHelper.ListTaskLogs(loggingClient, new List<string> { "cloudsharpsystems" });
            return result;
        }

    }

}
