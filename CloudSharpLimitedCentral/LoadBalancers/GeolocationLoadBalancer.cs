using APIConnector.IPHelper;
using APIConnector.Model;
using AuxiliaryClassLibrary.Network;
using CloudSharpSystemsWeb.LoadBalancers;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Geolocation;
using System.Collections;
using System.Text.Json;

namespace CloudSharpLimitedCentral.LoadBalancers
{
    public class GeolocationLoadBalancer : ILoadBalancer
    {
        public GeolocationLoadBalancer(AppDBMainContext db_context) : base(db_context) { }

        protected override async Task<TB_USER_SESSION> ExecuteBalance(AppDBMainContext db_context, string SiteID, HttpContext ClientContext)
        {
            //string client_IP = ClientContext.Connection.RemoteIpAddress!.ToString();
            //string trace_ID = ClientContext.TraceIdentifier;
            //long packets_size = HttpRequestHeaderHelper.ASSUMED_HEADER_SIZE + (ClientContext.Request.ContentLength ?? 0);

            var clientInfo = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(ClientContext);

            TB_USER_SESSION new_session = await NetworkLoadBalancingDataContext.LoadBalanceProcedure(db_context, SiteID, clientInfo.client_IP, clientInfo.trace_ID, (int)clientInfo.request_size);

            // test
            //client_IP = "129.7.0.124";
            double shortest_distance = Double.MaxValue;

            // Get geolocation info from Client IP:
            var ClientIPGeoInfo = await LocalIP.GetIPGeoLocation(clientInfo.client_IP);

            // Get geolocation info from host server data list:
            var server_location_details = 
                (await NetworkLoadBalancingDataContext.GetServerLoadDistributionFunction(db_context, SiteID))
                .Where(detail => (detail.IP_STATUS ?? "").Equals("NORMAL") || (detail.IP_STATUS ?? "").Equals("ERROR"));

            if (ClientIPGeoInfo.status!.Equals("success"))
            {
                // Compute the host with shortest distance from client:
                foreach (var detail in server_location_details)
                {
                    IPGeoLocationObject HostIPGeoInfo = JsonSerializer.Deserialize<IPGeoLocationObject>(detail.RACK_CODE!)!;
                    double distance = GeoCalculator.GetDistance(ClientIPGeoInfo.lat, ClientIPGeoInfo.lon, HostIPGeoInfo.lat, HostIPGeoInfo.lon, 1);
                    if (distance < shortest_distance && detail.NET_LOAD_CAPACITY - detail.RESOURCE_LOAD > clientInfo.request_size)
                    {
                        new_session.HOST_IP = detail.HOST_IP;
                        shortest_distance = distance;
                    }
                }

            }
            else if (server_location_details.Any()) 
            {
                new_session.HOST_IP = server_location_details.First().HOST_IP;
            }

            
            if (String.IsNullOrEmpty(new_session.HOST_IP)) new_session.RESOURCE_UNIT = -1;

            return new_session;
        }

    }
}
