using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class ProductsServerContext
    {
        /*
        public static async Task<TB_SERVER> UpdateComputerIP(AppDBMainContext DBContext, string SiteID)
        {
            return await DBContext.Websites.FirstAsync(w => w.SITE_ID == SiteID);
        }
        */

        public static async Task<TB_SERVER> GetServerInfoBySiteAndIP(AppDBMainContext DBContext, string SiteID, string IP) 
        {
            var host = await NetworkWebsiteHostContext.GetWebsiteHostDetailByIP(DBContext, SiteID, IP);
            string serialNo = host.SERIAL_NO!;
            return await DBContext.Servers.FirstAsync(server => server.SERIAL_NO!.Equals(serialNo));
        }

        public static async Task ResetServerCapacity(AppDBMainContext DBContext, string hostIP, int capacity, float preset_error_rate, string editBy)
        {

			var host_IP_param = DBContext.SQLParameterType("HOST_IP", System.Data.DbType.String, hostIP, System.Data.ParameterDirection.Input);
			var capacity_param = DBContext.SQLParameterType("CAPACITY", System.Data.DbType.Int32, capacity, System.Data.ParameterDirection.Input);
			var edit_by_param = DBContext.SQLParameterType("EDIT_BY", System.Data.DbType.String, editBy, System.Data.ParameterDirection.Input);
			var preset_error_rate_param = DBContext.SQLParameterType("PRESET_ERROR_RATE", System.Data.DbType.Decimal, preset_error_rate, System.Data.ParameterDirection.Input);

			var parameters = new System.Data.Common.DbParameter[] { host_IP_param, capacity_param, edit_by_param, preset_error_rate_param };
            string formatted_sql_str = DBContext.FormatExecSPSQL("NETWORK.RESET_SERVER_CAPACITY", parameters);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, parameters);
        }



        public static IQueryable<T_SERVER_DETAIL>? GetServerDetailsQuery(AppDBMainContext DBContext, string site_ID)
        {
            var query = from detail in DBContext.GET_SERVER_DETAILS(site_ID)
                        select detail;
            return query;
            
        }

        public static async Task<List<T_SERVER_DETAIL> > GetHealthyServerDetails(AppDBMainContext DBContext, string site_ID) 
        {
            var query = from detail in GetServerDetailsQuery(DBContext, site_ID)
                        where detail.SERVER_STATUS == "RUNNING" && detail.IP_STATUS == "NORMAL"
                        select detail;

            var lst = await query.ToListAsync();
            return lst;
        }


        public static async Task<List<V_SERVER_USAGE> > GetServerUsageLogistics(AppDBMainContext DBContext) {
            var query = from row_logistic in DBContext.ServerUsageView
                        orderby (row_logistic.SERVER_STATUS == "RUNNING"? 1 : -1) descending, row_logistic.SERVER_STATUS, row_logistic.SERIAL_NO
                        select row_logistic;
            var lst = await query.ToListAsync();
            return lst;
        }

    }
}
