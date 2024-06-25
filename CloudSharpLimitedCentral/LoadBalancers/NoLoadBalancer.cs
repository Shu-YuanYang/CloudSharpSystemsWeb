using AuxiliaryClassLibrary.Network;
using DBConnectionLibrary;
using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.Models;

namespace CloudSharpSystemsWeb.LoadBalancers
{
    public class NoLoadBalancer : ILoadBalancer
    {

        public NoLoadBalancer(AppDBMainContext db_context) : base(db_context) { }

        protected override async Task<TB_USER_SESSION> ExecuteBalance(AppDBMainContext db_context, string SiteID, HttpContext ClientContext)
        {
            var clientInfo = HttpRequestHeaderHelper.GetClientHttpInfoFromHttpContext(ClientContext);

            TB_USER_SESSION new_session = await NetworkLoadBalancingDataContext.LoadBalanceProcedure(db_context, SiteID, clientInfo.client_IP, clientInfo.trace_ID, (int)clientInfo.request_size);

            return new_session;
        }
    }
}
