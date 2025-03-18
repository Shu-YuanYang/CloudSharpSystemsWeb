using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class NetworkLoadBalancingDataContext
    {


        public static async Task<TB_USER_SESSION> LoadBalanceProcedure(AppDBMainContext DBContext, string site_ID, string client_IP, string thread_ID, int resource_size)
        {
			var site_ID_param = DBContext.SQLParameterType("SITE_ID", System.Data.DbType.String, site_ID, System.Data.ParameterDirection.Input);
			var client_IP_param = DBContext.SQLParameterType("CLIENT_IP", System.Data.DbType.String, client_IP, System.Data.ParameterDirection.Input);
			var thread_ID_param = DBContext.SQLParameterType("CLIENT_THREAD_ID", System.Data.DbType.String, thread_ID, System.Data.ParameterDirection.Input);
			var resource_size_param = DBContext.SQLParameterType("RESOURCE_SIZE", System.Data.DbType.Int32, resource_size, System.Data.ParameterDirection.Input);
			var host_IP_param = DBContext.SQLParameterType("HOST_IP", System.Data.DbType.String, null, System.Data.ParameterDirection.Output); // Size is not handled directly in the new format
			var resource_unit_param = DBContext.SQLParameterType("RESOURCE_UNIT", System.Data.DbType.Int32, null, System.Data.ParameterDirection.Output);
            host_IP_param.Size = 20;

            var parameters = new System.Data.Common.DbParameter[] { site_ID_param, client_IP_param, thread_ID_param, resource_size_param, host_IP_param, resource_unit_param };
            string formatted_sql_str = DBContext.FormatExecSPSQL("NETWORK.LOAD_BALANCE", parameters);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, parameters);

            var session = new TB_USER_SESSION
            {
                SESSION_ID = null, //DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("D"),
                CLIENT_IP = client_IP,
                THREAD_ID = thread_ID,
                HOST_IP = host_IP_param.Value!.ToString(),
                RESOURCE_UNIT = (int)(resource_unit_param.Value!),
                //CLIENT_LOCATION = session_obj.CLIENT_LOCATION,
                //REQUESTED_TIME = session_obj.REQUESTED_TIME,
                RESOURCE_SIZE = resource_size,
                //EDIT_BY = session_obj.EDIT_BY,
                //EDIT_TIME = session_obj.EDIT_TIME,
                IS_VALID = 'Y'
            };

            return session;
        }




        public static async Task LoadBalanceResetProcedure(AppDBMainContext DBContext, string site_ID, string algorithm, int max_search_count, string edit_by)
        {
			var site_ID_param = DBContext.SQLParameterType("SITE_ID", System.Data.DbType.String, site_ID, System.Data.ParameterDirection.Input);
			var algorithm_param = DBContext.SQLParameterType("ALGORITHM", System.Data.DbType.String, algorithm, System.Data.ParameterDirection.Input);
			var max_search_count_param = DBContext.SQLParameterType("MAX_SEARCH_COUNT", System.Data.DbType.Int32, max_search_count, System.Data.ParameterDirection.Input);
			var edit_by_param = DBContext.SQLParameterType("EDIT_BY", System.Data.DbType.String, edit_by, System.Data.ParameterDirection.Input);

            var parameters = new System.Data.Common.DbParameter[] { site_ID_param, algorithm_param, max_search_count_param, edit_by_param };
            string formatted_sql_str = DBContext.FormatExecSPSQL("NETWORK.LOAD_BALANCE_RESET", parameters);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, parameters);
        }


        public static async Task<List<T_SERVER_LOAD_DISTRIBUTION>> GetServerLoadDistributionFunction(AppDBMainContext DBContext, string site_ID)
        {
            var query = from dist in DBContext.GET_SERVER_LOAD(site_ID)
                        where dist.SERVER_STATUS == "RUNNING"
                        orderby dist.NET_LOAD_CAPACITY
                        select dist;
            
            var lst = await query.ToListAsync();
            return lst;
        }


    }
}
