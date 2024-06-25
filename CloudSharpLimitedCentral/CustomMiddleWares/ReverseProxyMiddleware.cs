using AuxiliaryClassLibrary.DateTimeHelper;
using AuxiliaryClassLibrary.Network;
using Azure.Core;
using CloudSharpLimitedCentralWeb.Models;
using CloudSharpSystemsWeb.LoadBalancers;
using CloudSharpSystemsWeb.Network;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
// Reverse Proxy Middleware Class rewritten by Andrea Chiarelli
// Posted on: https://auth0.com/blog/building-a-reverse-proxy-in-dot-net-core/
// Main modification in this file is the InvokeAsync function


using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace CustomMiddleWares
{



    public class ReverseProxyMiddleware
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;

        private class _LoadBalanceOutput 
        { 
            public string? selected_host_IP { get; set; }
            public DateTime requested_time { get; set; }
        }






        
        public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
        }

        // Shu-Yuan Yang 10/06/2023 added dependency injection of
        // IConfiguration (Program Configuration) and
        // AppDBMainContext (Database context)
        public async Task InvokeAsync(HttpContext context, IConfiguration config, AppDBMainContext db_context)
        {

            // Get Site config:
            string site_ID = config["SiteConfig:TestController"]!;


            // Forward by load balancing:
            var forwarded_paths = config.GetSection(ReverseProxyConfig.REVERSE_PROXY_CONFIG).GetRequiredSection(ReverseProxyConfig.FORWARDED_PATHS).Get<List<string> >();
            if (forwarded_paths.Any(p => context.Request.Path.StartsWithSegments(p))) 
            {
                await LoadBalanceForwardSingleAsync(context, site_ID, db_context);
                return;
            }
            
            // If the request is not forwarded, process within the central server controllers:
            await _nextMiddleware(context);

        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);

            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
              !HttpMethods.IsHead(requestMethod) &&
              !HttpMethods.IsDelete(requestMethod) &&
              !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }
            
            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            
            
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }



        // Shu-Yuan Yang modified 10/06/2023
        // Construct URL with load balancing
        private Uri BuildTargetUri(HttpRequest request, string target_IP)
        {
            
            Uri targetUri;
            string target_url_str = APIConnector.RESTAPIConnector.ConstructRESTBaseURL(target_IP);
            target_url_str = target_url_str.Remove(target_url_str.Length - 1, 1);
            string path = (request.Path.HasValue)? request.Path.Value : "/error";
            targetUri = new Uri(target_url_str + path);
            
            /*
            if (request.Path.StartsWithSegments("/googleforms", out var remainingPath))
            {
                targetUri = new Uri("https://docs.google.com/forms" + remainingPath);
            }
            */

            return targetUri;
        }







        /******************************************** Shu-Yuan Yang Central Server Functions ************************************************/

        private async Task LoadBalanceForwardSingleAsync(HttpContext context, string site_ID, AppDBMainContext db_context) 
        {
            // 1. Execute load balancing transaction:
            _LoadBalanceOutput load_balance_result = await this.LoadBalanceTransactionAsync(context, site_ID, db_context);


            // 2. If load balancer could not decide on a host IP, reroute to resource-unavailable handler: 
            if (String.IsNullOrEmpty(load_balance_result.selected_host_IP))
            {
                this.ProcessInvalidLoadBalanceSelection(context);
                await _nextMiddleware(context);
                return;
            }


            // 3. Attach Client IP, HttpContext Trace ID, and Request Received Time to the forwarded request header:
            this.AddForwardHeaders(context, load_balance_result.requested_time);
            // context.Features.Set<IHttpRequestTimeFeature>(httpRequestTimeFeature); // Record requested time, disabled


            // 4. Forward request, and process response message:
            using (var responseMessage = await this.ForwardRequestAsync(context, load_balance_result.selected_host_IP))
            {
                string host_status = responseMessage.IsSuccessStatusCode ? "NORMAL" : "ERROR";
                string log_message = responseMessage.IsSuccessStatusCode ? "Host responded normally." : "Host responded with an error.";
                await NetworkWebsiteHostContext.InsertHostStatusLog(db_context, load_balance_result.selected_host_IP, host_status, context.TraceIdentifier, log_message, "CloudSharpSystemsLimitedCentral");
                if (!responseMessage.IsSuccessStatusCode)
                {
                    this.ProcessForwardServerError(context, load_balance_result.selected_host_IP);
                    await _nextMiddleware(context);
                    return;
                }

                context.Response.StatusCode = (int)responseMessage.StatusCode;
                CopyFromTargetResponseHeaders(context, responseMessage);
                await responseMessage.Content.CopyToAsync(context.Response.Body);

            }
        }

        /*
        private async Task ForwardAllAsync(HttpContext context, string site_ID, AppDBMainContext db_context)
        {
            // Get all host list by site_ID
            var hostList = await NetworkWebsiteHostContext.GetWebsiteHostsBySiteID(db_context, site_ID);
            DateTime current_time = await DBTransactionContext.DBGetDateTime(db_context);

            this.AddForwardHeaders(context, current_time);

            hostList = hostList.Where(h => {
                var ping_result = IPValidator.Ping_IPV4(h.HOST_IP!);
                return ping_result == System.Net.NetworkInformation.IPStatus.Success;
            }).ToList();

            List<string> responses = new List<string>();
            foreach (var host in hostList) 
            {
                using (var responseMessage = await this.ForwardRequestAsync(context, host.HOST_IP!))
                {
                    string response = await responseMessage.Content.ReadAsStringAsync();
                    responses.Add(response);
                }
            }

            

            context.Response.StatusCode = 200;

            string serialised_response = JsonSerializer.Serialize(responses);
            await Microsoft.AspNetCore.Http.HttpResponseWritingExtensions.WriteAsync(context.Response, serialised_response);
            
        }
        */

        private async Task<_LoadBalanceOutput> LoadBalanceTransactionAsync(HttpContext context, string site_ID, AppDBMainContext db_context) 
        {
            //var httpRequestTimeFeature = new HttpRequestTimeFeature(); // disabled

            // Do Load Balancing
            string? selected_host_IP = "";
            var session_obj = new TB_USER_SESSION();
            DateTime requested_time = DateTime.Now;
            await DBTransactionContext.DBTransact(db_context, async (app_db_context, transaction) =>
            {

                await NetworkUserSessionContext.LockUserSessionTable(app_db_context);


                // i. Get maximum retries count:
                string max_search_control_value = await AppDataContext.GetAppDataControlValue(db_context, "CloudSharpSystemsWeb", "LOAD_BALANCING_ALGORITHM", "MAX_HOST_SEARCH_COUNT", "");
                int load_balance_max_search_count = Int32.Parse(max_search_control_value);


                // ii. Get current time:
                DateTime current_time = await DBTransactionContext.DBGetDateTime(app_db_context);
                //httpRequestTimeFeature.RequestTime = current_time; // diabled
                requested_time = current_time;


                // Retry load balancing
                while (String.IsNullOrEmpty(selected_host_IP) && 0 < load_balance_max_search_count)
                {
                    // iii. Call Load Balance procedure:
                    session_obj = await ILoadBalancer.Balance(db_context, site_ID, context);
                    selected_host_IP = session_obj.HOST_IP;
                    if (String.IsNullOrEmpty(selected_host_IP)) break; // load balancer cannot find appropriate host to handle request, break retries.

                    // iv. Ping host to ensure reacheability. Update host status to "UNRESPONSIVE" if pinging failed.
                    var ping_result = IPValidator.Ping_IPV4(selected_host_IP);
                    if (ping_result != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        await NetworkWebsiteHostContext.UpdateHostStatus(db_context, selected_host_IP, "80", "UNRESPONSIVE", context.TraceIdentifier, "Host is unreachable.", "CloudSharpSystemsLimitedCentral", 0);
                        //await NetworkWebsiteHostContext.InsertHostStatusLog(db_context, selected_host_IP, "UNRESPONSIVE", context.TraceIdentifier, "Host is unreachable.", "CloudSharpSystemsLimitedCentral");
                        selected_host_IP = null;
                    }

                    // v. Decrement retry quota:
                    --load_balance_max_search_count;
                }
            });


            // vi. Return selected host IP and requested time:
            return new _LoadBalanceOutput {
                selected_host_IP = selected_host_IP,
                requested_time = requested_time
            };
        }


        private void ProcessInvalidLoadBalanceSelection(HttpContext context)
        {
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = new PathString("/SystemError/resource_unavailable");
        }

        private void ProcessForwardServerError(HttpContext context, string host_IP) 
        {
            context.Request.Method = HttpMethods.Get;
            context.Request.Path = new PathString($"/SystemError/caught_server_error/{host_IP}");
        }

        private void AddForwardHeaders(HttpContext context, DateTime requested_time) 
        {
            context.Request.Headers.Add(ReverseProxyHttpHeader.Client_IP, context.Connection.RemoteIpAddress!.ToString());
            context.Request.Headers.Add(ReverseProxyHttpHeader.Trace_Identifier, context.TraceIdentifier);
            context.Request.Headers.Add(ReverseProxyHttpHeader.Requested_Time, TimestampHelper.ToPreciseFormatString(requested_time));
        }


        private async Task<HttpResponseMessage> ForwardRequestAsync(HttpContext context, string selected_host_IP) 
        {
            var targetUri = BuildTargetUri(context.Request, selected_host_IP);

            //if (targetUri != null)
            //{
            var targetRequestMessage = CreateTargetMessage(context, targetUri);

            return await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            
            //}
        }

        /******************************************** End Central Server Functions **********************************************************/




    }
}