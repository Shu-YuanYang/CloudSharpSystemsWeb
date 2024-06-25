using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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

            var site_ID_param = new SqlParameter
            {
                ParameterName = "SITE_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = site_ID
            };

            var client_IP_param = new SqlParameter
            {
                ParameterName = "CLIENT_IP",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = client_IP
                //"73.166.146.251"
            };

            var thread_ID_param = new SqlParameter
            {
                ParameterName = "CLIENT_THREAD_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = thread_ID
            };

            var resource_size_param = new SqlParameter
            {
                ParameterName = "RESOURCE_SIZE",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Input,
                Value = resource_size
            };

            var host_IP_param = new SqlParameter
            {
                ParameterName = "HOST_IP",
                DbType = System.Data.DbType.String,
                Size = 20,
                Direction = System.Data.ParameterDirection.Output
            };

            var resource_unit_param = new SqlParameter
            {
                ParameterName = "RESOURCE_UNIT",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };

            var parameters = new object[] { site_ID_param, client_IP_param, thread_ID_param, resource_size_param, host_IP_param, resource_unit_param };


            await DBContext.Database.ExecuteSqlRawAsync("EXEC NETWORK.LOAD_BALANCE @SITE_ID, @CLIENT_IP, @CLIENT_THREAD_ID, @RESOURCE_SIZE, @HOST_IP OUTPUT, @RESOURCE_UNIT OUTPUT", parameters);

            var session = new TB_USER_SESSION
            {
                SESSION_ID = null, //DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("D"),
                CLIENT_IP = client_IP,
                THREAD_ID = thread_ID,
                HOST_IP = host_IP_param.Value.ToString(),
                RESOURCE_UNIT = (int)(resource_unit_param.Value),
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

            var site_ID_param = new SqlParameter
            {
                ParameterName = "SITE_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = site_ID
            };

            var algorithm_param = new SqlParameter
            {
                ParameterName = "ALGORITHM",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = algorithm
            };

            var max_search_count_param = new SqlParameter
            {
                ParameterName = "MAX_SEARCH_COUNT",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Input,
                Value = max_search_count
            };

            var edit_by_param = new SqlParameter
            {
                ParameterName = "EDIT_BY",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = edit_by
            };

            var parameters = new object[] { site_ID_param, algorithm_param, max_search_count_param, edit_by_param };

            await DBContext.Database.ExecuteSqlRawAsync("EXEC NETWORK.LOAD_BALANCE_RESET @SITE_ID, @ALGORITHM, @MAX_SEARCH_COUNT, @EDIT_BY", parameters);
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
