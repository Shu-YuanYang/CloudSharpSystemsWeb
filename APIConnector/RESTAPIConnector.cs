using APIConnector.Model;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIConnector
{
    public class RESTAPIConnector
    {

        // Reference: https://stackoverflow.com/questions/9620278/how-do-i-make-calls-to-a-rest-api-using-c
        public static async Task<string> CallRestAPIAsync(RESTAPIInputModel input)
        {
            try {
                return await TryCallRestAPIAsync(input, false);
            } catch(Exception e) {
                return "Exception occurred: " + e.Message;
            }

        }

        public static async Task<HttpResponseMessage> CallRestAPIResponseAsync(RESTAPIInputModel input) 
        {
            try
            {
                return await TryRestAPIResponseAsync(input);
            }
            catch
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                
                //string serialised_response = JsonSerializer.Serialize(responses);
                //await Microsoft.AspNetCore.Http.HttpResponseWritingExtensions.WriteAsync(context.Response, serialised_response);

                return response;
            }
        }

        public static void Redirect(HttpContext context, RESTAPIInputModel input) {
            try
            {
                TryRedirect(context, input);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public static async Task<string> TryCallRestAPIAsync(RESTAPIInputModel input, bool test)
        {
            /*
            using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
        "todos",
        new Todo(UserId: 9, Id: 99, Title: "Show extensions", Completed: false));

            response.EnsureSuccessStatusCode()
                .WriteRequestToConsole();

            var todo = await response.Content.ReadFromJsonAsync<Todo>();
            Console.WriteLine($"{todo}\n");

            var jsonResponse = await response.Content.ReadAsStringAsync();
            */

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(input.URL!);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(input.ContentType!));
            if (!String.IsNullOrEmpty(input.AccessToken)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", input.AccessToken!);

            // List data response.
            string response_obj_string;
            HttpResponseMessage response;
            if (input.APIType == RESTAPIType.GET)
            {
                response = await client.GetAsync(input.Parameters);
            }
            else if (input.APIType == RESTAPIType.POST)
            {
                //response = await client.PostAsJsonAsync(input.Parameters, input.InputObject);
                if (input.ContentType == "application/json")
                    response = await client.PostAsJsonAsync(input.Parameters, input.InputObject);
                else if (input.ContentType == "application/x-www-form-urlencoded")
                    response = await client.PostAsync(input.Parameters, new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)input.InputObject!));
                else
                    throw new Exception("New API content type has not been developed yet");
            }
            else 
            {
                throw new Exception("New API methods have not been developed yet!");
            }
            
            if (response.IsSuccessStatusCode)
            {
                response_obj_string = await response.Content.ReadAsStringAsync();
            }
            else
            {
                response_obj_string = $"{(int)response.StatusCode} ({response.ReasonPhrase})";
            }
            if (test) Console.WriteLine(response_obj_string);

            // Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();

            return response_obj_string;
        }


        public static async Task<HttpResponseMessage> TryRestAPIResponseAsync(RESTAPIInputModel input)
        {
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(input.URL!);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(input.ContentType!));

            // List data response.
            HttpResponseMessage response = new HttpResponseMessage();
            if (input.APIType == RESTAPIType.GET)
            {
                response = await client.GetAsync(input.Parameters);
            }
            else if (input.APIType == RESTAPIType.POST)
            {
                if (input.ContentType == "application/json")
                    response = await client.PostAsJsonAsync(input.Parameters, input.InputObject);
                else if (input.ContentType == "application/x-www-form-urlencoded")
                    response = await client.PostAsync(input.Parameters, new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)input.InputObject!));
                else
                    throw new Exception("New API content type has not been developed yet");
            }
            else
            {
                throw new Exception("New API methods have not been developed yet!");
            }

            // Dispose once all HttpClient calls are complete. This is not necessary if the containing object will be disposed of; for example in this case the HttpClient instance will be disposed automatically when the application terminates so the following call is superfluous.
            client.Dispose();

            return response;
        }


        public static void TryRedirect(HttpContext context, RESTAPIInputModel input) {
            if (input.URL!.EndsWith("/")) input.URL = input.URL.Substring(0, input.URL.Length - 1);
            if (!input.Parameters!.StartsWith("?") && !input.Parameters!.StartsWith("#")) input.Parameters = "?" + input.Parameters;
            context.Response.Redirect(input.URL + input.Parameters);
        }



        public static string ConstructRESTBaseURL(string IP, bool is_http = true) {
            return "http" + (is_http ? "" : "s") + "://" + IP + "/";
        }


        public static string ConstructRESTPathURL(string base_url, string? subpath) {
            string full_url = base_url;
            if (!String.IsNullOrEmpty(subpath)) { 
                if (base_url.EndsWith("/") && subpath.StartsWith("/")) subpath = subpath.Substring(1);
                else if (!base_url.EndsWith("/") && !subpath.StartsWith("/")) subpath = "/" + subpath;
                full_url += subpath;
            }
            return full_url;
        }


        



    }
}
