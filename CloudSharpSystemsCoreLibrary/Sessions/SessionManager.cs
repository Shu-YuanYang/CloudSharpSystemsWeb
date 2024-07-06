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

namespace CloudSharpSystemsCoreLibrary.Sessions
{
    public class SessionManager
    {

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

        public async Task<TB_USER_SESSION> UpdateSession(string hostIP, ClientHttpContextInfo client_info, GoogleAPIOauth2TokenResponse token_response, GoogleAPIOAuth2UserInfo user_info, string appID, string updateType = "SIGNED_IN")
        {
            // Search for existing session and identity
            // by user_info.email
            var identity_lst = await AppUserContext.GetUserIdentities(this._app_db_main_context, new TB_APP_USER_IDENTITY { 
                IDENTITY_PROVIDER = "GOOGLE",
                USERNAME = user_info.email
            });
            if (!identity_lst.Any()) throw new InvalidCredentialException($"Identity not found with provider/username {"GOOGLE"}/{user_info.email} in CloudSharp Systems!");
            var identity = identity_lst.First();

            string session_id = SecurityStateGenerator.GenerateRandomState(SecurityStateGenerator.Salt.Byte64Base64);
            var new_session = new TB_USER_SESSION
            {
                SESSION_ID = session_id,
                CLIENT_IP = client_info.client_IP, // GCP IP, NOT ACCURATE
                THREAD_ID = identity.USERID,
                HOST_IP = hostIP,
                RESOURCE_UNIT = 0,
                CLIENT_LOCATION = "protected",
                REQUESTED_TIME = DateTime.Now,
                RESOURCE_SIZE = (int)client_info.request_size,
                EDIT_BY = appID, //identity.USERID,
                //EDIT_TIME = DateTime.Now,
                IS_VALID = 'Y',
                SESSION_ITEMS = new List<TB_USER_SESSION_ITEM> {
                        new TB_USER_SESSION_ITEM {
                            SESSION_ID = session_id,
                            ITEM_NAME = "IDENTITY/GOOGLE",
                            ITEM_DESCRIPTION = JsonSerializer.Serialize(token_response),
                            ITEM_SIZE = 0,
                            ITEM_ROUTE = "GOOGLE",
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
                await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, "", "", identity.USERID!, hostIP);

                // create a new session with identity item
                new_session = await NetworkUserSessionContext.InsertNewUserSession(app_db_context, new_session);

                // Write signed in message to system log
                string oauth_response = JsonSerializer.Serialize(user_info);
                await AppDataContext.WriteSystemLog(app_db_context, new TB_CENTRAL_SYSTEM_LOG
                {
                    APP_ID = appID,
                    SYSTEM_NAME = hostIP,
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
                await NetworkUserSessionContext.InvalidateUserSessions(app_db_context, session.SESSION_ID!, "", session.THREAD_ID!, "");
            });
        }


        public async Task WriteLogOutSystemLog(string hostIP, ClientHttpContextInfo client_info, TB_USER_SESSION session, TB_USER_SESSION_ITEM session_item, string appID, string oauth2_logout_response) {
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
                    SYSTEM_NAME = hostIP,
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
