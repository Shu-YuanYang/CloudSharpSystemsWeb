using APIConnector.Model;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Text.Json;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using System.Security.Authentication;
using Google.Cloud.Storage.V1;
using Microsoft.VisualBasic;
using Google;

namespace APIConnector.GoogleCloud
{
    public class GCPCredentialsHelper
    {
        private readonly ExternalAPIMap _external_api_map;

        public const string IDENTITY_PROVIDER = "GOOGLE";



        public const string OAUTH2_LOGIN_URL = "https://accounts.google.com/o/oauth2/v2/auth";
        public const string OAUTH2_REFRESH_URL = "https://oauth2.googleapis.com/token";
        public const string OAUTH2_REVOKE_URL = "https://oauth2.googleapis.com/revoke";


        public GCPCredentialsHelper(ExternalAPIMap externalAPIMap) 
        {
            this._external_api_map = externalAPIMap;
        }

        // Code below copied and modified from: https://cloud.google.com/storage/docs/samples/storage-generate-signed-url-v4
        public static async Task<string> GenerateV4SignedReadUrl(string scope_API_url, ExternalAPIPathConfig scope_path_config, GCPServiceAccountSecretKeyObject key_obj, string bucketName, string objectName, int duration_in_seconds = 300)
        {
            string serialized_credential_json = JsonSerializer.Serialize(key_obj);

            string full_scope_url = RESTAPIConnector.ConstructRESTPathURL(scope_API_url, scope_path_config.path);

            var scopes = new string[] { full_scope_url /*"https://www.googleapis.com/auth/devstorage.read_only"*/ };
            GoogleCredential service_account_credential = GoogleCredential.FromJson(serialized_credential_json).CreateScoped(scopes);
            UrlSigner urlSigner = UrlSigner.FromCredential(service_account_credential/*GoogleCredential.GetApplicationDefault()*/);
            
            // V4 is the default signing version.
            string url = await urlSigner.SignAsync(bucketName, objectName, TimeSpan.FromSeconds(duration_in_seconds), HttpMethod.Get, SigningVersion.V4);
            //Console.WriteLine("Generated GET signed URL:");
            //Console.WriteLine(url);
            //Console.WriteLine("You can use this URL with any user agent, for example:");
            //Console.WriteLine($"curl '{url}'");
            return url;
        }


        public static async Task UploadFileToStorage(string scope_API_url, ExternalAPIPathConfig scope_path_config, GCPServiceAccountSecretKeyObject key_obj, string bucketName, string objectName, MemoryStream memoryStream)
        {
            string serialized_credential_json = JsonSerializer.Serialize(key_obj);

            string full_scope_url = RESTAPIConnector.ConstructRESTPathURL(scope_API_url, scope_path_config.path);

            var scopes = new string[] { full_scope_url /*"https://www.googleapis.com/auth/devstorage.read_write"*/ };
            GoogleCredential service_account_credential = GoogleCredential.FromJson(serialized_credential_json).CreateScoped(scopes);
            var storage = await StorageClient.CreateAsync(service_account_credential);
            /*
            try
            {
                if (overwrite)
                {
                    var existing_obj = await storage.GetObjectAsync(bucketName, objectName);
                    throw 
                }
            }
            catch (GoogleApiException ex) {
                if (ex.Error.Code != 404) throw ex;
            }
            */
            var obj = await storage.UploadObjectAsync(bucketName, objectName, "application/octet-stream", memoryStream);
        }


        public async Task<TokenResponse> GetOAuth2TokenByCode(GCPOAuth2ClientSecretKeyObject key_obj, GoogleAPIOAuth2LoginCodeObject authorization_code_obj)
        {
            // create authorization code flow with clientSecrets
            GoogleAuthorizationCodeFlow authorizationCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ProjectId = key_obj.web!.project_id,
                
                //DataStore = new FileDataStore(dataStoreFolder),
                ClientSecrets = new ClientSecrets()
                {
                    ClientId = key_obj.web!.client_id,
                    ClientSecret = key_obj.web!.client_secret
                },
                IncludeGrantedScopes = true,
                Prompt = authorization_code_obj.prompt,
                Scopes = new string[] {
                    "openid",
                    RESTAPIConnector.ConstructRESTPathURL(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_user_profile")!.path),
                    RESTAPIConnector.ConstructRESTPathURL(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_scope_user_email_address")!.path)
                }
            });

            var tokenResponse = await authorizationCodeFlow.ExchangeCodeForTokenAsync(
                authorization_code_obj.authuser, // user for tracking the userId on our backend system
                authorization_code_obj.code,
                key_obj.web.redirect_uris![0], // redirect_uri can not be empty. Must be one of the redirects url listed in your project in the api console
                CancellationToken.None
            );

            return tokenResponse;
        }


        public async Task VerifyOauth2TokenAccessToken(GCPOAuth2ClientSecretKeyObject key_obj, string access_token) {
            var token_info = await GoogleAPIHelper.GetTokenInfo(this._external_api_map.GoogleAPI!.url!, this._external_api_map.GoogleAPI!.api!.GetValueOrDefault("oauth2_tokeninfo")!, access_token);
            if (key_obj.web!.client_id != token_info.aud) throw new InvalidCredentialException("Invalid token audience!");
        }

        public async Task<string> RefreshOAuth2AccessToken(GCPOAuth2ClientSecretKeyObject key_obj, string refresh_token)
        {
            RESTAPIInputModel model = new RESTAPIInputModel
            {
                URL = GCPCredentialsHelper.OAUTH2_REFRESH_URL,
                ContentType = "application/json",
                APIType = RESTAPIType.POST,
                InputObject = new {
                    client_id = key_obj.web!.client_id,
                    client_secret = key_obj.web!.client_secret,
                    refresh_token = refresh_token,
                    grant_type = "refresh_token"
                } 
            };

            string response = await RESTAPIConnector.CallRestAPIAsync(model);
            return response;
        }

        public async Task<string> RevokeOAuth2AccessToken(string access_token) {

            RESTAPIInputModel model = new RESTAPIInputModel { 
                URL = GCPCredentialsHelper.OAUTH2_REVOKE_URL,
                ContentType = "application/json", //"application/x-www-form-urlencoded",
                APIType = RESTAPIType.POST,
                InputObject = new { token = access_token } //new[] { new KeyValuePair<string, string>("token", access_token) }
            };

            string response = await RESTAPIConnector.CallRestAPIAsync(model);
            return response;
        }

    }


}
