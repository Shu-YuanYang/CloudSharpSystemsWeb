using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class NetworkWebsiteHostContext
    {
        public static async Task<List<TB_WEBSITE_HOST>> GetWebsiteHostsBySiteID(AppDBMainContext DBContext, string SiteID)
        {
            return await DBContext.WebsiteHosts.Where(h => h.SITE_ID == SiteID && !h.STATUS!.Equals("DISABLED")).ToListAsync();
        }

        public static async Task<TB_WEBSITE_HOST> GetWebsiteHostDetailByIP(AppDBMainContext DBContext, string SiteID, string HostIP)
        {
            if (SiteID.IsNullOrEmpty())
            {
                return await DBContext.WebsiteHosts.FirstAsync(h => h.HOST_IP == HostIP);
            }
            else
            {
                return await DBContext.WebsiteHosts.FirstAsync(h => h.SITE_ID == SiteID && h.HOST_IP == HostIP);
            }
        }

        public static async Task UpdateComputerIP(AppDBMainContext DBContext, string SerialNo, string new_IP)
        {
			var SNparam = DBContext.SQLParameterType("SN", System.Data.DbType.String, SerialNo, System.Data.ParameterDirection.Input);
			var ipparam = DBContext.SQLParameterType("NEW_IP", System.Data.DbType.String, new_IP, System.Data.ParameterDirection.Input);

            var parameters = new System.Data.Common.DbParameter[] { SNparam, ipparam };
            string formatted_sql_str = DBContext.FormatExecSPSQL("NETWORK.UPDATE_HOST_IP_BY_SN", parameters);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, parameters);
        }


        public static async Task UpdateHostStatus(AppDBMainContext DBContext, string host_IP, string port, string status, string trace_ID, string additional_message, string edit_by, double latency)
        {
			var host_IP_param = DBContext.SQLParameterType("HOST_IP", System.Data.DbType.String, host_IP, System.Data.ParameterDirection.Input);
			var port_param = DBContext.SQLParameterType("PORT", System.Data.DbType.String, port, System.Data.ParameterDirection.Input);
			var status_param = DBContext.SQLParameterType("STATUS", System.Data.DbType.String, status, System.Data.ParameterDirection.Input);
			var edit_by_param = DBContext.SQLParameterType("EDIT_BY", System.Data.DbType.String, edit_by, System.Data.ParameterDirection.Input);
			var trace_ID_param = DBContext.SQLParameterType("TRACE_ID", System.Data.DbType.String, trace_ID, System.Data.ParameterDirection.Input);
			var additional_message_param = DBContext.SQLParameterType("INPUT_MESSAGE", System.Data.DbType.String, additional_message, System.Data.ParameterDirection.Input);
			var latency_param = DBContext.SQLParameterType("LATENCY", System.Data.DbType.Double, latency, System.Data.ParameterDirection.Input);

			var parameters = new System.Data.Common.DbParameter[] { host_IP_param, port_param, status_param, edit_by_param, trace_ID_param, additional_message_param, latency_param };
            string formatted_sql_str = DBContext.FormatExecSPSQL("NETWORK.UPDATE_HOST_STATUS", parameters);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, parameters);

        }


        // Get portions of the log: 
        public static async Task<List<TB_HOST_STATUS_LOG>> GetRecentHostStatusLogRecords(AppDBMainContext DBContext, string host_IP, int time_offset_in_hours) {
            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
            DateTime threshold_time = current_time.AddHours(-time_offset_in_hours);
            var result_lst = await DBContext.HostStatusLogs.Where(l => l.HOST_IP == host_IP && l.EDIT_TIME >= threshold_time).OrderBy(l => l.EDIT_TIME).ToListAsync();
            return result_lst;
        }

        // Get latency statistics:
        public static async Task<List<T_HOST_LATENCY_STATISTICS>[]> GetRecentHostLatencyStatistics(AppDBMainContext DBContext, int TimeOffSetHours, int TimeIntervalMinutes, string[] hostIPs) {
            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
            var queries = hostIPs.Select((hostIP) =>
            {
                var query = from stat in DBContext.GET_DB_HOST_LATENCY_STATISTICS_TEST(TimeOffSetHours, TimeIntervalMinutes, hostIP, current_time)
                            orderby stat.END_TIME
                            select stat;
                
                return query;
            });

            var results = queries.Select((query) => {
                try { return query.ToList(); }
                catch { return new List<T_HOST_LATENCY_STATISTICS>(); }
            });

            return results.ToArray();
        }

        // Record server operation status data
        public static async Task InsertHostStatusLog(AppDBMainContext DBContext, string host_IP, string status, string trace_ID, string message, string edit_by)
        {

			var host_IP_param = DBContext.SQLParameterType("HOST_IP", System.Data.DbType.String, host_IP, System.Data.ParameterDirection.Input);
			var host_status_param = DBContext.SQLParameterType("HOST_STATUS", System.Data.DbType.String, status, System.Data.ParameterDirection.Input);
			var trace_ID_param = DBContext.SQLParameterType("TRACE_ID", System.Data.DbType.String, trace_ID, System.Data.ParameterDirection.Input);
			var input_message_param = DBContext.SQLParameterType("INPUT_MESSAGE", System.Data.DbType.String, message, System.Data.ParameterDirection.Input);
			var edit_by_param = DBContext.SQLParameterType("EDIT_BY", System.Data.DbType.String, edit_by, System.Data.ParameterDirection.Input);

			var proc_params = new System.Data.Common.DbParameter[] { host_IP_param, host_status_param, trace_ID_param, input_message_param, edit_by_param };
            string formatted_sql_str = DBContext.FormatExecSPSQL("NETWORK.INSERT_HOST_STATUS_LOG", proc_params);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, proc_params);
            
        }



    }
}
