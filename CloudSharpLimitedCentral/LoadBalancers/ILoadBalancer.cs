using AuxiliaryClassLibrary.IO;
using CloudSharpLimitedCentral.LoadBalancers;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace CloudSharpSystemsWeb.LoadBalancers
{
    public abstract class ILoadBalancer
    {
        /*
        private static readonly SortedDictionary<string, string> LoadBalancerMapping = new SortedDictionary<string, string>{
            { "HOME_PAGE", "WEIGHTED_ROUND_ROBIN" },
            { "SUPPLY_CHAIN_PAGE", "LEAST_CONNECTIONS" },
            { "NETWORK_MONITOR_PAGE", "NONE" }
        };
        */
        private readonly AppDBMainContext _db_context;

        public ILoadBalancer(AppDBMainContext db_context) {
            _db_context = db_context; 
        }

        public static ILoadBalancer Instantiate(AppDBMainContext db_context, string LoadBalancerName/*, IConfiguration config*/) {
            if (LoadBalancerName == "WEIGHTED_ROUND_ROBIN") return new WeightedRoundRobinLoadBalancer(db_context);
            if (LoadBalancerName == "LEAST_CONNECTIONS") return new LeastConnectionsLoadBalancer(db_context);
            if (LoadBalancerName == "GEOLOCATION_GLOBAL") return new GeolocationLoadBalancer(db_context);
            if (LoadBalancerName == "WEIGHTED_FAULT_AVOIDANCE") return new WeightedFaultAvoidanceLoadBalancer(db_context);
            return new NoLoadBalancer(db_context); // No load balancing
            /*
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetTypes().First(t => t.Name == "WeightedRoundRobinLoadBalancer");
            ILoadBalancer b = (ILoadBalancer) Activator.CreateInstance(type)!;
            */
        }

        public static int ComputeResourceSize(Object data) {
            return 0;
        }

        public static async Task<TB_USER_SESSION> Balance(AppDBMainContext db_context, string SiteID, HttpContext ClientContext) 
        {
            //ClientData.RESOURCE_SIZE = new Packet(ClientData).ComputePacketSize();
            //long packet_size = ClientContext.Request.ContentLength!.Value;


            TB_WEBSITE website_obj = await NetworkWebsiteContext.GetWebsiteByID(db_context, SiteID);
            ILoadBalancer LoadBalancer = Instantiate(db_context, website_obj.LOAD_BALANCING_ALGORITHM!);
            Task<TB_USER_SESSION> new_session_object = LoadBalancer.ExecuteBalance(db_context, SiteID, ClientContext);
            // return session info with attributes: client IP, thread ID, host IP, resource unit, resource size
            return await new_session_object;
        }

        protected abstract Task<TB_USER_SESSION> ExecuteBalance(AppDBMainContext db_context, string SiteID, HttpContext ClientContext);


        public static bool ValidateSelection(string host_IP, ref Object response) {
            if (String.IsNullOrEmpty(host_IP))
            {
                response = new {
                    connection_data = new 
                    {
                        connection_status = "failure",
                        resource_unit = -1,
                        resource_size = -1,
                        resource_request_response = "resource denied"
                    },
                    processing_delay = 0f,
                    message = "Server resources are currently unavailable"
                };
                return false;
            }

            return true;
        }

    }

}
