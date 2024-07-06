
using APIConnector;
using APIConnector.GoogleCloud;
using APIConnector.Model;
using AuxiliaryClassLibrary.Network;
using CloudSharpSystemsCoreLibrary.Security;
using CloudSharpSystemsCoreLibrary.Sessions;
using CloudSharpLimitedCentral.Controllers;
using DBConnectionLibrary;
using DBConnectionLibrary.Models;
using Microsoft.Extensions.Options;
using System.Security.Authentication;

namespace CloudSharpLimitedCentral.CustomMiddleWares
{
    public class RedirectMiddleWare
    {

        private const string USER_STATE = "USER_STATE";
        private const string LOGIN_STATE = "LOGIN_STATE";
        private const string ORIGIN_RETURN_URL = "ORIGIN_RETURN_URL";

        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;


        public RedirectMiddleWare(RequestDelegate nextMiddleware)
        {
            this._nextMiddleware = nextMiddleware;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration config, IOptions<GCPOAuth2ClientSecretKeyObject> GCPOAuth2CredentialsClientSecretAccessor, AppDBMainContext db_context)
        {
            //var clientInfo = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(context);

            string? url = context.Request.Path.Value;
            string _app_id = config[TemplateController.APP_ID_CONFIG_KEY]!;
            var _external_api_map = config.GetSection(TemplateController.EXTERNAL_API_CONFIG_KEY).Get<ExternalAPIMap>();


            GCPOAuth2ClientSecretKeyObject gcp_client_secrets = GCPOAuth2CredentialsClientSecretAccessor.Value;
            GCPCredentialsHelper gcp_credentials_helper = new GCPCredentialsHelper(_external_api_map);
            string[] GCPScopes = new string[] {
                "openid",
                RESTAPIConnector.ConstructRESTPathURL(_external_api_map.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_user_profile")!.path),
                RESTAPIConnector.ConstructRESTPathURL(_external_api_map.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_user_email_address")!.path),
            };
            if (url!.Contains("/auth/gcp/login"))
            {
                this.RedirectToGoogleSignIn(context, gcp_client_secrets, GCPScopes);
                return;
            }

            if (url!.Contains("/auth/gcp/authenticate")) {
                var token_info = await this.GCPAuthenticate(context, gcp_client_secrets, gcp_credentials_helper);
                var user_info = await GoogleAPIHelper.GetUserInfo(_external_api_map.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_userinfo")!, token_info.access_token!);
                var session_data = await this.SaveSession(context, config["HostConfig"]!, token_info, user_info, db_context, _app_id);
                this.RedirectBackToOrigin(context, gcp_client_secrets, session_data.SESSION_ID!);
                return;
            }

            // If the request is not forwarded, process within the central server controllers:
            await _nextMiddleware(context);

        }



        private void RedirectToGoogleSignIn(HttpContext context, GCPOAuth2ClientSecretKeyObject client_secrets, string[] scopes) {
            string login_url = GCPCredentialsHelper.OAUTH2_LOGIN_URL; 
            
            string scope = string.Join(" ", scopes);
            string access_type = "offline";
            string include_granted_scopes = "true";
            string response_type = "code";  //"token";
            string state = SecurityStateGenerator.GenerateRandomState(SecurityStateGenerator.Salt.Byte32Base64); //"stateparameterpassthroughvalue" + TimestampHelper.ToUniversalISOFormatString(DateTime.Now); // TODO: generate random and store in session:
            string redirect_uri = client_secrets.web!.redirect_uris![0];
            string client_id = client_secrets.web!.client_id!;

            var url_query = context.Request.Query;
            string decoded_return_url = Uri.UnescapeDataString(url_query["return_redirect_uri"]!);
            context.Session.SetString(ORIGIN_RETURN_URL, decoded_return_url); // url to redirect to at the end of the authentication process
            context.Session.SetString(USER_STATE, url_query["state"]!); // user state to prevent CSRF attack on the last return trip
            context.Session.SetString(LOGIN_STATE, state);

            scope = Uri.EscapeDataString(scope);
            state = Uri.EscapeDataString(state);
            redirect_uri = Uri.EscapeDataString(redirect_uri);
            client_id = Uri.EscapeDataString(client_id);

            var input_model = new RESTAPIInputModel
            {
                URL = login_url,
                Parameters = $"scope={scope}&access_type={access_type}&include_granted_scopes={include_granted_scopes}&response_type={response_type}&state={state}&redirect_uri={redirect_uri}&client_id={client_id}"
            };

            RESTAPIConnector.Redirect(context, input_model);
            //context.Response.Redirect(full_auth_url);
        }


        private async Task<GoogleAPIOauth2TokenResponse> GCPAuthenticate(HttpContext context, GCPOAuth2ClientSecretKeyObject client_secrets, GCPCredentialsHelper gcp_credentials_helper) {
            string? gcp_preset_login_state = context.Session.GetString(LOGIN_STATE);
            var url_query = context.Request.Query;

            // Validate ephemeral state to prevent CSRF attack:
            if (gcp_preset_login_state != url_query["state"]) { 
                throw new InvalidCredentialException ("Google redirection blocked: state from sign-in provider does not match the one sent by client!");
            }

            GoogleAPIOAuth2LoginCodeObject test_code_obj = new GoogleAPIOAuth2LoginCodeObject
            {
                code = url_query["code"],
                scope = url_query["scope"], 
                authuser = url_query["authuser"],
                prompt = url_query["prompt"]
            };
            
            var response = await gcp_credentials_helper.GetOAuth2TokenByCode(client_secrets, test_code_obj);

            await gcp_credentials_helper.VerifyOauth2TokenAccessToken(client_secrets, response.AccessToken);

            return new GoogleAPIOauth2TokenResponse
            {
                access_token = response.AccessToken,
                token_type = response.TokenType,
                expires_in = response.ExpiresInSeconds!.Value,
                refresh_token = response.RefreshToken,
                scope = response.Scope,
                id_token = response.IdToken,
                issued_utc = response.IssuedUtc
            };
        }

        private async Task<TB_USER_SESSION> SaveSession(HttpContext context, string hostIP, GoogleAPIOauth2TokenResponse token_response, GoogleAPIOAuth2UserInfo user_info, AppDBMainContext db_context, string appID) {
            var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(context);
            SessionManager session_manager = new SessionManager(db_context);
            var session_data = await session_manager.UpdateSession(hostIP, client_info, token_response, user_info, appID);
            return session_data;
        }

        private void RedirectBackToOrigin(HttpContext context, GCPOAuth2ClientSecretKeyObject client_secrets, string session_id) 
        {
            
            string origin_return_uri = context.Session.GetString(ORIGIN_RETURN_URL)!;
            string user_state = context.Session.GetString(USER_STATE)!;


            // Validate return url to prevent dangerous redirects:
            Uri origin_return_uri_obj = new Uri(origin_return_uri);
            bool is_origin_return_uri_allowed = client_secrets.web!.redirect_uris!
                .Any((uri) => {
                    var uri_obj = new Uri(uri);
                    return uri_obj.IsBaseOf(origin_return_uri_obj);
                });
            if (!is_origin_return_uri_allowed) 
                throw new InvalidCredentialException($"The return redirect URL is not allowed by the service provider on Google Cloud Platform! Return url: {origin_return_uri}. Valid Url: {string.Join(",", client_secrets.web!.redirect_uris!)}");

            session_id = Uri.EscapeDataString(session_id);
            user_state = Uri.EscapeDataString(user_state);
            var input_model = new RESTAPIInputModel
            {
                URL = origin_return_uri, // external_api_map.CloudSharpVisualDataDashboard!.url,
                Parameters = $"state={user_state}#session_id={session_id}"
            };

            RESTAPIConnector.Redirect(context, input_model);
        }

    }
}
