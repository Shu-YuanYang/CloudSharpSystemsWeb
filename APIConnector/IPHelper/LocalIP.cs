using APIConnector.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIConnector.IPHelper
{
    public class LocalIP
    {
        // reference: https://www.ipify.org/
        public static async Task<string?> GetMyIP() {
            string response = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = "https://api.ipify.org/",
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = "?format=json"
            });
            

            //HttpClient client = new HttpClient();

            //string response = await client.GetStringAsync("https://api.ipify.org/?format=json");
            //IpAddress results = JsonConvert.DeserializeObject<IPObject>(jsonData);

            IPObject? my_ip_obj = System.Text.Json.JsonSerializer.Deserialize<IPObject>(response);
            return my_ip_obj?.ip;
        }

        public static async Task<IPGeoLocationObject> GetIPGeoLocation(string IP) {
            string response = await RESTAPIConnector.CallRestAPIAsync(new APIConnector.Model.RESTAPIInputModel
            {
                URL = "http://ip-api.com/json/",
                ContentType = "application/json",
                APIType = RESTAPIType.GET,
                Parameters = IP
            });

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            IPGeoLocationObject ip_geolocation_obj = System.Text.Json.JsonSerializer.Deserialize<IPGeoLocationObject>(response, options)!;
            return ip_geolocation_obj;
        }

    }
}
