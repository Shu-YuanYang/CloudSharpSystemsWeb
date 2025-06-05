using APIConnector.GoogleCloud;
using APIConnector.Model;
using AuxiliaryClassLibrary.Network;
using Azure.Core;
using CloudSharpSystemsCoreLibrary.Security;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace CloudSharpSystemsCoreLibrary.Sessions
{
    public class SessionManager
    {
        public const string CLOUDSHARP_IDENTITY_PROVIDER = "CLOUDSHARPSYSTEMS";


		protected readonly AppDBMainContext _app_db_main_context;

        public SessionManager(AppDBMainContext appDBMainContext) {
            this._app_db_main_context = appDBMainContext;
        }


        public async Task<TB_USER_SESSION> GetSessionByAuthorizationHeader(HttpRequest request, bool filter_by_identity_provider, string identity_provider) {
            string authorization_header = HttpRequestHeaderHelper.GetAuthorizationAccessToken(request)!;
            if (string.IsNullOrEmpty(authorization_header)) throw new InvalidCredentialException("Authorization token not found!");

            // query for existing session:
            var sessions = await NetworkUserSessionContext.GetUserSessions(this._app_db_main_context, new TB_USER_SESSION
            {
                SESSION_ID = authorization_header,
                IS_VALID = 'Y'
            });
            if (!sessions.Any()) throw new InvalidCredentialException("Invalid user token to fetch app user session!");
            var session = sessions.First();

            // query for identity session item:
            if (filter_by_identity_provider)
            {
                string item_name = $"IDENTITY/{GCPCredentialsHelper.IDENTITY_PROVIDER}";
                session.SESSION_ITEMS = session.SESSION_ITEMS!.Where(item => item.ITEM_NAME == item_name).ToList();
                if (!session.SESSION_ITEMS.Any()) throw new InvalidCredentialException("Invalid identity provider in session record for this user token!");
            }

            return session;
        }

        public async Task<TB_USER_SESSION> GetLinkedSessionByAuthorizationHeader(HttpRequest request, string identity_provider) {
            var session = await this.GetSessionByAuthorizationHeader(request, false, "");
            if (session.SESSION_ITEMS?.Any(i => i.ITEM_NAME == $"IDENTITY/{identity_provider}") ?? false) return session;

			var sessions = await NetworkUserSessionContext.GetUserSessions(this._app_db_main_context, new TB_USER_SESSION
			{
				THREAD_ID = session.THREAD_ID,
				IS_VALID = 'Y'
			});
			if (!sessions.Any()) throw new InvalidCredentialException("Invalid user token to fetch app user session!");
            
            // query for identity session item:
            sessions = sessions.Where(s => s.SESSION_ITEMS?.Any(i => i.ITEM_NAME == $"IDENTITY/{identity_provider}") ?? false);
            if (!sessions.Any()) throw new InvalidCredentialException("Invalid identity provider in session record for this user token!");

            return sessions.First();
		}

		public async Task<IEnumerable<TB_USER_SESSION>> GetSessionsByClientInfo(HttpRequest request, ClientHttpContextInfo client_info)
		{
            // Validate incoming client IP:
            if (String.IsNullOrEmpty(client_info.client_IP)) throw new AuthenticationException("Client IP cannot be empty when querying for sessions by client info!");

			// query for existing session:
			var sessions = await NetworkUserSessionContext.GetUserSessions(this._app_db_main_context, new TB_USER_SESSION
			{
				CLIENT_IP = client_info.client_IP,
				IS_VALID = 'Y'
			});
			
			return sessions;
		}

        public async Task<TB_APP_USER_IDENTITY> GetUserIdentity(string identity_provider, string? username, string? user_id) {
			var identity_lst = await AppUserContext.GetUserIdentities(this._app_db_main_context, new TB_APP_USER_IDENTITY
			{
                USERID = user_id,
				IDENTITY_PROVIDER = identity_provider,
				USERNAME = username
			});
            if (!identity_lst.Any())
            {
                if (username == null) throw new InvalidCredentialException($"Identity not found with provider/userid {"GOOGLE"}/{user_id} in CloudSharp Systems!");
                else throw new InvalidCredentialException($"Identity not found with provider/username {"GOOGLE"}/{username} in CloudSharp Systems!");
			}
            var identity = identity_lst.First();
            return identity;
		}

		public async Task<TB_USER_SESSION> UpdateSession(ClientHttpContextInfo client_info, GoogleAPIOauth2TokenResponse token_response, GoogleAPIOAuth2UserInfo user_info, string appID, string updateType = "SIGNED_IN")
        {
            // Search for existing session and identity
            // by user_info.email
            var identity = await GetUserIdentity(GCPCredentialsHelper.IDENTITY_PROVIDER, user_info.email!, "");

			// Find current session, and recover refresh token if necessary:
			IEnumerable<TB_USER_SESSION> current_sessions = await NetworkUserSessionContext.GetUserSessions(this._app_db_main_context, new TB_USER_SESSION { THREAD_ID = identity.USERID, /*HOST_IP = client_info.client_host,*/ IS_VALID = 'Y' });
            IEnumerable<TB_USER_SESSION> host_sessions = current_sessions.Where(s => s.HOST_IP == client_info.client_host);

			//TB_USER_SESSION host_session = (host_sessions.Any) .First();
            TB_USER_SESSION_ITEM host_session_item;
			string item_route;
			if (host_sessions.Any()) // found existing session on the client host
			{
                var host_session_items = host_sessions.First().SESSION_ITEMS?.Where(i => i.ITEM_NAME!.Contains("IDENTITY/"));
                host_session_item = (host_session_items != null && host_session_items.Any())? host_session_items.First() : new TB_USER_SESSION_ITEM { };
				item_route = String.IsNullOrEmpty(host_session_item.ITEM_ROUTE) ? identity.IDENTITY_PROVIDER! : host_session_item.ITEM_ROUTE;
            }
            else if (current_sessions.Any()) // use cloudsharp session attached to an existing session from another host
			{
                host_session_item = new TB_USER_SESSION_ITEM { };
                item_route = CLOUDSHARP_IDENTITY_PROVIDER;   
            }
            else // no existing session found
            {
                host_session_item = new TB_USER_SESSION_ITEM { };
                item_route = identity.IDENTITY_PROVIDER!;
			}

            /*
            var current_session_items_raw = current_sessions.Select(session => {
                var session_items = session.SESSION_ITEMS?.Where(item => item.ITEM_NAME == item_name);
                if (session_items == null) session_items = new List<TB_USER_SESSION_ITEM>();
                return session_items;
            });
            IEnumerable<TB_USER_SESSION_ITEM> current_session_items = new List<TB_USER_SESSION_ITEM>();
            if (current_session_items_raw.Any()) current_session_items = current_session_items_raw.Aggregate((lst1, lst2) => lst1.Concat(lst2));
            */

            if (String.IsNullOrEmpty(token_response.refresh_token) && !String.IsNullOrEmpty(host_session_item.SESSION_ID)) {
                var token_info = JsonSerializer.Deserialize<GoogleAPIOauth2TokenResponse>(host_session_item.ITEM_DESCRIPTION!);
                token_response.refresh_token = token_info!.refresh_token!;
            }


            // Generate new session object:
            string session_id = SecurityStateGenerator.GenerateRandomState(SecurityStateGenerator.Salt.Byte64Base64);
            var new_session = new TB_USER_SESSION
            {
                SESSION_ID = session_id,
                CLIENT_IP = client_info.client_IP, // GCP IP, NOT ACCURATE
                THREAD_ID = identity.USERID,
                HOST_IP = client_info.client_host, //hostIP,
                RESOURCE_UNIT = 0,
                CLIENT_LOCATION = "protected",
                REQUESTED_TIME = DateTime.UtcNow, // Use UTC time!
                RESOURCE_SIZE = (int)client_info.request_size,
                EDIT_BY = appID, //identity.USERID,
                //EDIT_TIME = DateTime.Now,
                IS_VALID = 'Y',
                SESSION_ITEMS = new List<TB_USER_SESSION_ITEM> {
                    new TB_USER_SESSION_ITEM {
                        SESSION_ID = session_id,
                        ITEM_NAME = $"IDENTITY/{item_route}",
                        ITEM_DESCRIPTION = JsonSerializer.Serialize(token_response),
                        ITEM_SIZE = 0,
                        ITEM_ROUTE = item_route,
                        ITEM_POLICY = token_response.access_token,
                        EXPIRATION_TIME = token_response.issued_utc!.Value.AddSeconds((double)token_response.expires_in!),
                        EDIT_BY = appID //identity.USERID,
                        //EDIT_TIME = DateTime.Now
                    }
                }
            };

            // Session update transaction:
            await DBTransactionContext.DBTransact(this._app_db_main_context, async (app_db_context, transaction) =>
            {
                // If session exists, invalidate and preserve other identity info
                await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, null, null, identity.USERID!, client_info.client_host);

                // create a new session with identity item
                new_session = await NetworkUserSessionContext.InsertNewUserSession(app_db_context, new_session);

                // Write signed in message to system log
                string oauth_response = JsonSerializer.Serialize(user_info);
                await AppDataContext.WriteSystemLog(app_db_context, new TB_CENTRAL_SYSTEM_LOG
                {
                    APP_ID = appID,
                    SYSTEM_NAME = client_info.client_host,
                    TRACE_ID = client_info.trace_ID,
                    RECORD_TYPE = "GOOD",
                    RECORD_KEY = "OAUTH2",
                    RECORD_VALUE1 = updateType,
                    RECORD_VALUE2 = $"CloudSharp UID: {identity.USERID}",
                    RECORD_VALUE3 = $"Identity Username: {identity.USERNAME}",
                    RECORD_VALUE4 = $"Session ID: {session_id}",
                    RECORD_VALUE5 = $"OAuth2 Provider: {identity.IDENTITY_PROVIDER}",
                    RECORD_MESSAGE = $"OAuth2 Response: {oauth_response[0..Math.Min(oauth_response.Length, 500)]}",
                    RECORD_NOTE = "OAuth2 Security Tracking",
                    EDIT_BY = appID
                });
            });

            return new_session;
        }



        public async Task InvalidateSession(TB_USER_SESSION session) {
            // Session deletion transaction:
            await DBTransactionContext.DBTransact(this._app_db_main_context, async (app_db_context, transaction) =>
            {
				//await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, session.SESSION_ID!, "", session.THREAD_ID!, "");
				await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, session.SESSION_ID!, null, session.THREAD_ID!, null);
			});
        }

		public async Task InvalidateSessions(IEnumerable<TB_USER_SESSION> sessions)
		{
			// Session deletion transaction:
			await DBTransactionContext.DBTransact(this._app_db_main_context, async (app_db_context, transaction) =>
			{
				//await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, session.SESSION_ID!, "", session.THREAD_ID!, "");
				foreach (var session in sessions) await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, session.SESSION_ID!, null, session.THREAD_ID!, null);
			});
		}


		public async Task WriteLogOutSystemLog(ClientHttpContextInfo client_info, TB_USER_SESSION session, TB_USER_SESSION_ITEM session_item, string appID, string oauth2_logout_response) {
            await DBTransactionContext.DBTransact(this._app_db_main_context, async (app_db_context, transaction) =>
            {
                var user_identity = (await AppUserContext.GetUserIdentities(app_db_context, new TB_APP_USER_IDENTITY
                {
                    IDENTITY_PROVIDER = session_item.ITEM_ROUTE,
                    USERID = session.THREAD_ID
                })).First();

                await AppDataContext.WriteSystemLog(app_db_context, new TB_CENTRAL_SYSTEM_LOG
                {
                    APP_ID = appID,
                    SYSTEM_NAME = client_info.client_host,
                    TRACE_ID = client_info.trace_ID,
                    RECORD_TYPE = "GOOD",
                    RECORD_KEY = "OAUTH2",
                    RECORD_VALUE1 = "SIGNED_OUT",
                    RECORD_VALUE2 = $"CloudSharp UID: {user_identity.USERID}",
                    RECORD_VALUE3 = $"Identity Username: {user_identity.USERNAME}",
                    RECORD_VALUE4 = $"Session ID: {session.SESSION_ID}",
                    RECORD_VALUE5 = $"OAuth2 Provider: {user_identity.IDENTITY_PROVIDER}",
                    RECORD_MESSAGE = $"OAuth2 Pesponse: {oauth2_logout_response[0..Math.Min(oauth2_logout_response.Length, 500)]}",
                    RECORD_NOTE = "OAuth2 Security Tracking",
                    EDIT_BY = appID
                });
            });

        }

    }


}
