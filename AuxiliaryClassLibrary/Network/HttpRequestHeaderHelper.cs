using Microsoft.AspNetCore.Http;

namespace AuxiliaryClassLibrary.Network
{

    public struct ClientHttpContextInfo 
    {
        public string client_IP;
        public string trace_ID;
        public long request_size;
    }


    public class HttpRequestHeaderHelper
    {
        public static long ASSUMED_HEADER_SIZE => 40;

        public static bool TryParseHeaderValueFirst(HttpRequest request, string header_key, out string? value) 
        {
            value = null;
            Microsoft.Extensions.Primitives.StringValues vals;
            bool is_parsed = request.Headers.TryGetValue(header_key, out vals);
            if (is_parsed)
            {
                string list_str = vals.First()!;
                value = list_str.Split(',')[0].Trim();
            }

            return is_parsed;
        }


        public static ClientHttpContextInfo GetClientHttpInfoFromHttpContext(HttpContext context)
        {
            return new ClientHttpContextInfo
            {
                client_IP = context.Connection.RemoteIpAddress!.ToString(),
                trace_ID = context.TraceIdentifier,
                request_size = HttpRequestHeaderHelper.ASSUMED_HEADER_SIZE + (context.Request.ContentLength ?? 0)
            };
        }

        public static string? GetAuthorizationAccessToken(HttpRequest request)
        {
            string? authorization_header;
            bool is_authorization_header_set = HttpRequestHeaderHelper.TryParseHeaderValueFirst(request, ReverseProxyHttpHeader.Authorization, out authorization_header);
            if (!String.IsNullOrEmpty(authorization_header))
            {
                string BEARER_PREFIX = "Bearer ";
                int position = authorization_header.IndexOf(BEARER_PREFIX);
                if (position >= 0)
                {
                    authorization_header = authorization_header.Substring(position + BEARER_PREFIX.Length);
                }
            }
            return authorization_header;
        }

    }
}
