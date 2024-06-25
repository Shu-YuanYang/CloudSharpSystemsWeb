using APIConnector.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIConnector.GoogleCloud
{
    public class GoogleAPIHelper
    {


        public static async Task<GoogleAPIOauth2TokenInfo> GetTokenInfo(string API_url, ExternalAPIPathConfig path_config, string access_token) {
            string full_url = RESTAPIConnector.ConstructRESTPathURL(API_url, path_config.path);

            var response_message = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = full_url,
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = "",
                AccessToken = access_token
            });

            var response = JsonSerializer.Deserialize<GoogleAPIOauth2TokenInfo>(response_message)!;

            return response;
        }

        public static async Task<GoogleAPIOAuth2UserInfo> GetUserInfo(string API_url, ExternalAPIPathConfig path_config, string access_token) {
            string full_url = RESTAPIConnector.ConstructRESTPathURL(API_url, path_config.path);

            var response_message = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = full_url,
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = "",
                AccessToken = access_token
            });

            var response = JsonSerializer.Deserialize<GoogleAPIOAuth2UserInfo>(response_message)!;

            return response;
        }

    }
}
