
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
using AuxiliaryClassLibrary.Functions;
using CloudSharpSystemsCoreLibrary.Messaging;
using SQLContext = DBConnectionLibrary.DBObjectContexts.SQL;
using MongoContext = DBConnectionLibrary.DBObjectContexts.Mongo;
using Azure.Communication.Email;

namespace CloudSharpLimitedCentral.CustomMiddleWares
{
    public class RedirectMiddleWare
    {

        private const string USER_STATE = "USER_STATE";
        private const string LOGIN_STATE = "LOGIN_STATE";
        private const string ORIGIN_RETURN_URL = "ORIGIN_RETURN_URL";
        private const string MESSAGE_ID = "MESSAGE_ID";

        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;


        public RedirectMiddleWare(RequestDelegate nextMiddleware)
        {
            this._nextMiddleware = nextMiddleware;
        }

        public async Task InvokeAsync(HttpContext context, IConfiguration config, IOptions<GCPOAuth2ClientSecretKeyObject> GCPOAuth2CredentialsClientSecretAccessor, AppDBMainContext db_context, AppDBMongoContext mongo_context)
        {
            //var clientInfo = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(context);

            string? url = context.Request.Path.Value;
            string _app_id = config[TemplateController.APP_ID_CONFIG_KEY]!;
            var _external_api_map = config.GetSection(TemplateController.EXTERNAL_API_CONFIG_KEY).Get<ExternalAPIMap>();

			string email_server_endpoint = config.GetSection("cyberlina")!.GetSection("email")!.GetValue<string>("connection_string")!;
			string sender_email = config.GetSection("cyberlina")!.GetSection("email")!.GetValue<string>("sender_email")!;
            var cyberlina_email_comm = new EmailCommService(EmailServiceProvider.AZURE, email_server_endpoint, sender_email);


			GCPOAuth2ClientSecretKeyObject gcp_client_secrets = GCPOAuth2CredentialsClientSecretAccessor.Value;
            GCPCredentialsHelper gcp_credentials_helper = new GCPCredentialsHelper(_external_api_map!);
            string[] GCPScopes = new string[] {
                "openid",
                RESTAPIConnector.ConstructRESTPathURL(_external_api_map!.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_user_profile")!.path),
                RESTAPIConnector.ConstructRESTPathURL(_external_api_map!.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_user_email_address")!.path),
            };
            if (url!.Contains("/auth/gcp/login"))
            {
                await this.RedirectToSignIn(context, _app_id, db_context, mongo_context, gcp_client_secrets, GCPScopes, cyberlina_email_comm);
                return;
            }

            if (url!.Contains("/auth/gcp/authenticate")) {
                var token_info = await this.GCPAuthenticate(context, gcp_client_secrets, gcp_credentials_helper);
                var user_info = await GoogleAPIHelper.GetUserInfo(_external_api_map.GoogleAPI!.url!, _external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_userinfo")!, token_info.access_token!);
                var session_data = await this.SaveSession(context, token_info, user_info, db_context, _app_id);
                this.RedirectBackToOrigin(context, gcp_client_secrets, session_data.SESSION_ID!);
                return;
            }

            if (url!.Contains("/auth/cloudsharp/authenticate")) {
                var token_info = await this.CloudSharpAuthenticate(context, mongo_context);
                var user_info = await this.CloudSharpGetUserInfo(token_info.id_token!, db_context);
                token_info.id_token = user_info.id;
				var session_data = await this.SaveSession(context, token_info, user_info, db_context, _app_id);
				this.RedirectBackToOrigin(context, gcp_client_secrets, session_data.SESSION_ID!);
				return;
			}

            // If the request is not forwarded, process within the central server controllers:
            await _nextMiddleware(context);

        }

        public async Task<GoogleAPIOAuth2UserInfo> CloudSharpGetUserInfo(string email, AppDBMainContext db_context) {
			SessionManager session_manager = new SessionManager(db_context);
            TB_APP_USER_IDENTITY identity = await session_manager.GetUserIdentity(GCPCredentialsHelper.IDENTITY_PROVIDER, email, null);
            return new GoogleAPIOAuth2UserInfo
            {
                email = identity.USERNAME,
                id = identity.USERID,
                verified_email = true
            };
		}


        public async Task<TB_EMAIL_HEADER> SendVerificationCodeToUser(string app_id, AppDBMainContext db_context, AppDBMongoContext mongo_context, TB_APP_USER_IDENTITY user_identity, string state, EmailCommService cyberlina_email_comm) {
            string verification_code = SecurityStateGenerator.GenerateRandomCode(6);
            var censored_email_parts = user_identity.USERNAME!.Split("@");
            string censored_username = censored_email_parts[0].Substring(0, 2) + "**" + censored_email_parts[0].Last() + "@" + censored_email_parts[1];

			string file_path = "Views/access_code_email.html";
			string content = System.IO.File.ReadAllText(file_path);
			//content = HTMLGenerator.GetHTMLFromTemplate(content, new object[] { email.recipients!.First() });

			CL_TEMPLATED_EMAIL email_content = new CL_TEMPLATED_EMAIL { 
                metadata = new { state = state, code = verification_code, expiration_time = DateTime.UtcNow.AddMinutes(10) },
                message = new TEMPLATED_EMAIL_MESSAGE {
                    subject = "CloudSharp Account Security Code",
                    body_template = content,
                    param_values = new Dictionary<string, string[]> {
                        { EncodingHelper.UTF8toHex(user_identity.USERNAME!), new string[] { censored_username, verification_code } }
                    }
                },
                recipients = new List<string> { user_identity.USERNAME! }
            };
            TB_EMAIL_HEADER email_header = new TB_EMAIL_HEADER
            {
                UPLOADED_BY = app_id,
                UPLOADED_TIME = DateTime.UtcNow,
                SCHEDULED_TIME = DateTime.UtcNow,
                NOTE = "CloudSharpLimitedCentral server issued per authentication request",
                STATUS = SQLContext.EmailContext.EmailDBStatus.SENT.ToString(),
                EDIT_BY = app_id
            };

            var cyberlina_sender = new EmailSender(cyberlina_email_comm);
            var email_status_tracker = cyberlina_sender.sendEmail(email_content);

            if (email_status_tracker.ResponseStatus != EmailSendStatus.Succeeded.ToString()) { 
                // throw ?
            }

			CL_TEMPLATED_EMAIL inserted_content = await MongoContext.EmailContext.AddTemplatedEmail(mongo_context, email_content);
			email_header.EMAIL_ID = inserted_content._id;
			TB_EMAIL_HEADER inserted_header = await SQLContext.EmailContext.InsertEmailSchedule(db_context, email_header);

            return inserted_header;
		}


        private void RedirectToGoogleSignIn(HttpContext context, GCPOAuth2ClientSecretKeyObject client_secrets, Dictionary<string, string> query)
        {
			string redirect_uri = client_secrets.web!.redirect_uris![0];
			string client_id = client_secrets.web!.client_id!;
			string scope = Uri.EscapeDataString(query["scope"]);
			string state = Uri.EscapeDataString(query["state"]);
			redirect_uri = Uri.EscapeDataString(redirect_uri);
			client_id = Uri.EscapeDataString(client_id);

			string login_url = GCPCredentialsHelper.OAUTH2_LOGIN_URL;
			var input_model = new RESTAPIInputModel
			{
				URL = login_url,
				Parameters = $"scope={scope}&access_type={query["access_type"]}&include_granted_scopes={query["include_granted_scopes"]}&response_type={query["response_type"]}&state={state}&redirect_uri={redirect_uri}&client_id={client_id}"
			};
			RESTAPIConnector.Redirect(context, input_model);
		}

        private void RedirectToCloudsharpCodeSignin(HttpContext context, Dictionary<string, string> query) {
			string login_url = "/central/cloudsharp_login";
			string state = Uri.EscapeDataString(query["state"]);
			string redirect_uri = Uri.EscapeDataString("/auth/cloudsharp/authenticate");
			var input_model = new RESTAPIInputModel
			{
				URL = login_url,
				Parameters = $"?verification_method=email&message_id={query["message_id"]}&state={state}&redirect_uri={redirect_uri}"
			};
			RESTAPIConnector.Redirect(context, input_model);
		}

		private async Task RedirectToSignIn(HttpContext context, string app_id, AppDBMainContext db_context, AppDBMongoContext mongo_context, GCPOAuth2ClientSecretKeyObject client_secrets, string[] scopes, EmailCommService cyberlina_email_comm) {

            var query = new Dictionary<string, string>();

            query["scope"] = string.Join(" ", scopes);
            query["access_type"] = "offline";
            query["include_granted_scopes"] = "true";
            query["response_type"] = "code";  //"token";
            query["state"] = SecurityStateGenerator.GenerateRandomState(SecurityStateGenerator.Salt.Byte32Base64); //"stateparameterpassthroughvalue" + TimestampHelper.ToUniversalISOFormatString(DateTime.Now); // TODO: generate random and store in session:
            
            var url_query = context.Request.Query;
            string decoded_return_url = Uri.UnescapeDataString(url_query["return_redirect_uri"]!);
            context.Session.SetString(ORIGIN_RETURN_URL, decoded_return_url); // url to redirect to at the end of the authentication process
            context.Session.SetString(USER_STATE, url_query["state"]!); // user state to prevent CSRF attack on the last return trip
            context.Session.SetString(LOGIN_STATE, query["state"]);

			var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(context);
			SessionManager session_manager = new SessionManager(db_context);
            var sessions = await session_manager.GetSessionsByClientInfo(context.Request, client_info);
            TB_USER_SESSION? session = null;
            bool is_reusing_login = false;
            
            // Determine if a different authorized site is requesting to reuse the session login:
            if (sessions.Any()) {
				session = sessions.First(s => s.SESSION_ITEMS!.Any(i => i.ITEM_NAME == $"IDENTITY/{GCPCredentialsHelper.IDENTITY_PROVIDER}"));
                if (session.HOST_IP != client_info.client_host) is_reusing_login = true;
			}
            // If resuing login, send an email to the user with a verification code, and redirect to cloudsharp verification page:
            if (is_reusing_login) {
				var user_identity = await session_manager.GetUserIdentity(GCPCredentialsHelper.IDENTITY_PROVIDER, "", session!.THREAD_ID);
                var email_header = await this.SendVerificationCodeToUser(app_id, db_context, mongo_context, user_identity, query["state"], cyberlina_email_comm);
                query["message_id"] = email_header.EMAIL_ID!;
				context.Session.SetString(MESSAGE_ID, email_header.EMAIL_ID!); // Record message ID in session
				this.RedirectToCloudsharpCodeSignin(context, query);
			}
            // Else, log in with Google Identity:
            else this.RedirectToGoogleSignIn(context, client_secrets, query);
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

        private async Task<GoogleAPIOauth2TokenResponse> CloudSharpAuthenticate(HttpContext context, AppDBMongoContext mongo_context)
        {
			string? cloudsharp_preset_login_state = context.Session.GetString(LOGIN_STATE);
			var url_query = context.Request.Query;

			// Validate ephemeral state to prevent CSRF attack:
			if (cloudsharp_preset_login_state != url_query["state"])
			{
				throw new InvalidCredentialException($"CloudSharp redirection blocked: state from sign-in provider does not match the one sent by client! session state: {cloudsharp_preset_login_state}, url state: {url_query["state"]}");
			}

            // Verify user from authenticated message:
            string auth_code = url_query["code"]!;
			string message_id = context.Session.GetString(MESSAGE_ID)!;

			var templated_emails = await MongoContext.EmailContext.GetTemplatedEmailsByIDs(mongo_context, new string[] { message_id });
            var verification_email = templated_emails.Single();
            dynamic email_metadata = verification_email.metadata!;
            string db_auth_code = email_metadata.auth_code;

            if (auth_code != db_auth_code) throw new UnauthorizedAccessException("Unrecognized authorization code!");
            

            return new GoogleAPIOauth2TokenResponse
            {
                access_token = SecurityStateGenerator.GenerateAuthenticationToken(),
                token_type = "Bearer",
                expires_in = 3599,
                refresh_token = SecurityStateGenerator.GenerateRandomState(SecurityStateGenerator.Salt.Byte32Base64),
                id_token = verification_email.recipients!.First(), // email
                issued_utc = DateTime.UtcNow
            };
		}

		private async Task<TB_USER_SESSION> SaveSession(HttpContext context, GoogleAPIOauth2TokenResponse token_response, GoogleAPIOAuth2UserInfo user_info, AppDBMainContext db_context, string appID) {
            var client_info = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(context);
            var uriBuilder = new UriBuilder(context.Session.GetString(ORIGIN_RETURN_URL)!);
            client_info.client_origin = $"{uriBuilder.Scheme}://{uriBuilder.Host}";
            client_info.client_host = uriBuilder.Host; // Rewrite host to redirect initiator due to Oauth2 origin interception
            SessionManager session_manager = new SessionManager(db_context);
            var session_data = await session_manager.UpdateSession(client_info, token_response, user_info, appID);
            return session_data;
        }

        private void RedirectBackToOrigin(HttpContext context, GCPOAuth2ClientSecretKeyObject client_secrets, string session_id) 
        {
            string origin_return_uri = context.Session.GetString(ORIGIN_RETURN_URL)!;
            string user_state = context.Session.GetString(USER_STATE)!;

            // Validate return url to prevent dangerous redirects:
            Uri origin_return_uri_obj = new Uri(origin_return_uri);
            bool is_origin_return_uri_allowed = client_secrets.web!.javascript_origins!
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
