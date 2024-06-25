using AuxiliaryClassLibrary.Network;
using CloudSharpSystemsWeb.LoadBalancers;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;
using DBConnectionLibrary;

namespace CloudSharpLimitedCentral.LoadBalancers
{
    public class WeightedFaultAvoidanceLoadBalancer : ILoadBalancer
    {
        public WeightedFaultAvoidanceLoadBalancer(AppDBMainContext db_context) : base(db_context) { }

        protected override async Task<TB_USER_SESSION> ExecuteBalance(AppDBMainContext db_context, string SiteID, HttpContext ClientContext)
        {
            var clientInfo = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(ClientContext);
            TB_USER_SESSION new_session = await NetworkLoadBalancingDataContext.LoadBalanceProcedure(db_context, SiteID, clientInfo.client_IP, clientInfo.trace_ID, (int)clientInfo.request_size);

            return new_session;
        }

    }
}
