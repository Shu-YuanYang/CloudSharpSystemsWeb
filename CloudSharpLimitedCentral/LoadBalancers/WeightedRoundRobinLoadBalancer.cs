using AuxiliaryClassLibrary.Network;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

namespace CloudSharpSystemsWeb.LoadBalancers
{

    /*
    -> compute weight ratio(scale based on load capacity)
    5, 4, 1, 3, 1

    2  1  3  1  2
    2  3     2
    2

    -> scale incoming request
    -> use weighted round robin to determine the server to route to: 
	    choose the current index if cumulative request weight<server weight
        update the total request weight of server - server weight if cumulative request weight >= server weight, and update index + 1
    */

    public class WeightedRoundRobinLoadBalancer : ILoadBalancer
    {
        
        public WeightedRoundRobinLoadBalancer(AppDBMainContext db_context) : base(db_context) { }

        protected override async Task<TB_USER_SESSION> ExecuteBalance(AppDBMainContext db_context, string SiteID, HttpContext ClientContext)
        {

            //string client_IP = ClientContext.Connection.RemoteIpAddress!.ToString();
            //string trace_ID = ClientContext.TraceIdentifier;
            //long packets_size = HttpRequestHeaderHelper.ASSUMED_HEADER_SIZE + (ClientContext.Request.ContentLength ?? 0);

            var clientInfo = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(ClientContext);

            TB_USER_SESSION new_session = await NetworkLoadBalancingDataContext.LoadBalanceProcedure(db_context, SiteID, clientInfo.client_IP, clientInfo.trace_ID, (int)clientInfo.request_size);
            
            return new_session;
        }


    }
}
