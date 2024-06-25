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
            var SNparam = new SqlParameter
            {
                ParameterName = "SN",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = SerialNo
            };

            var ipparam = new SqlParameter
            {
                ParameterName = "NEW_IP",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = new_IP
            };
            await DBContext.Database.ExecuteSqlRawAsync("EXEC NETWORK.UPDATE_HOST_IP_BY_SN @SN, @NEW_IP", new object[] { SNparam, ipparam });

        }


        public static async Task UpdateHostStatus(AppDBMainContext DBContext, string host_IP, string port, string status, string trace_ID, string additional_message, string edit_by, double latency)
        {
            var host_IP_param = new SqlParameter
            {
                ParameterName = "HOST_IP",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = host_IP
            };

            var port_param = new SqlParameter
            {
                ParameterName = "PORT",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = port
            };

            var status_param = new SqlParameter
            {
                ParameterName = "STATUS",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = status
            };

            var edit_by_param = new SqlParameter
            {
                ParameterName = "EDIT_BY",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = edit_by
            };

            var trace_ID_param = new SqlParameter
            {
                ParameterName = "TRACE_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = trace_ID
            };

            var additional_message_param = new SqlParameter
            {
                ParameterName = "INPUT_MESSAGE",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = additional_message
            };

            var latency_param = new SqlParameter
            {
                ParameterName = "LATENCY",
                DbType = System.Data.DbType.Double,
                Direction = System.Data.ParameterDirection.Input,
                Value = latency
            };

            await DBContext.Database.ExecuteSqlRawAsync("EXEC NETWORK.UPDATE_HOST_STATUS @HOST_IP, @PORT, @STATUS, @EDIT_BY, @TRACE_ID, @INPUT_MESSAGE, @LATENCY", new object[] { host_IP_param, port_param, status_param, edit_by_param, trace_ID_param, additional_message_param, latency_param });

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

            var host_IP_param = new SqlParameter
            {
                ParameterName = "@HOST_IP",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = host_IP
            };

            var host_status_param = new SqlParameter
            {
                ParameterName = "@HOST_STATUS",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = status
            };

            var trace_ID_param = new SqlParameter
            {
                ParameterName = "@TRACE_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = trace_ID
            };

            var input_message_param = new SqlParameter
            {
                ParameterName = "@INPUT_MESSAGE",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = message
            };

            var edit_by_param = new SqlParameter
            {
                ParameterName = "@EDIT_BY",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = edit_by
            };

            var proc_params = new object[] { host_IP_param, host_status_param, trace_ID_param, input_message_param, edit_by_param };
            await DBContext.Database.ExecuteSqlRawAsync("EXEC NETWORK.INSERT_HOST_STATUS_LOG @HOST_IP, @HOST_STATUS, @TRACE_ID, @INPUT_MESSAGE, @EDIT_BY", proc_params);
            
        }



    }
}
