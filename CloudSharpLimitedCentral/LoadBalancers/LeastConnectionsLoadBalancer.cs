using AuxiliaryClassLibrary.Network;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Hosting;

namespace CloudSharpSystemsWeb.LoadBalancers
{
    public class LeastConnectionsLoadBalancer : ILoadBalancer
    {

        public LeastConnectionsLoadBalancer(AppDBMainContext db_context) : base(db_context) { }

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
