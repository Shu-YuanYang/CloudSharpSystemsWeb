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

namespace CloudSharpSystemsWeb.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class GCPAssetController : TemplateController
    {
        

        public GCPAssetController(ILogger<TemplateController> logger, IConfiguration config, AppDBMainContext appDBMainContext, AppDBMongoContext appDBMongoContext, IOptions<GCPServiceAccountSecretKeyObject> GCPServiceAccountKeyAccessor) : base(logger, config, appDBMainContext, appDBMongoContext, GCPServiceAccountKeyAccessor)
        {
            
        }

        [HttpGet("get_storage_object_url")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<Object> GetGCPStorageObjectURL(string bucket, string directory, string object_name)
        {
            string signed_url = await GCPCredentialsHelper.GenerateV4SignedReadUrl(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_storage_read")!, this._gcp_service_account_key_obj, bucket, $"{directory}/{object_name}"/*"DATA_TABLE_MENU/cs-educator-posts-table.png"*/);
            return new { status = "OK", signed_url = signed_url };
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



        [HttpGet("identity_log_out")]
        [Produces("application/json")]
        [Consumes("application/json")]
        public async Task<GeneralAPIResponse> GCPIdentityLogOut() {

            var session = await this._session_manager.GetSessionByAuthorizationHeader(Request, true, GCPCredentialsHelper.IDENTITY_PROVIDER);
            var GCP_session_item = session.SESSION_ITEMS!.First();

            // Revoke access token and refresh token: Doc at https://developers.google.com/identity/protocols/oauth2/web-server#httprest_8 - Revoking a Token
            string logout_result = await this._gcp_credentials_helper.RevokeOAuth2AccessToken(GCP_session_item.ITEM_POLICY!);
            
            // Write system log to record token revoking:
            var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(Request.HttpContext);
            await this._session_manager.WriteLogOutSystemLog(this.MY_PUBLIC_IP, client_info, session, GCP_session_item, this.APP_ID, logout_result);

            // Session deletion transaction:
            await this._session_manager.InvalidateSession(session);

            return new GeneralAPIResponse { Status = "OK", Message = "Logged out." };
        }




    }

}
