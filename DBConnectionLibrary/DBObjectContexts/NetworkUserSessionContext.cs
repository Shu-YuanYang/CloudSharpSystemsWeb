using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class NetworkUserSessionContext
    {
        public static async Task LockUserSessionTable(AppDBMainContext DBContext) {
            await DBContext.Database.ExecuteSqlRawAsync("SELECT TOP 1 1 FROM NETWORK.TB_USER_SESSION WITH(TABLOCKX, HOLDLOCK)");
        }
        public static async Task<TB_USER_SESSION> InsertNewUserSession(AppDBMainContext DBContext, TB_USER_SESSION session_obj)
        {
            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
            session_obj.EDIT_TIME = current_time;

            await DBContext.UserSessions.AddAsync(session_obj);
            await DBContext.SaveChangesAsync();

            var saved_items = await NetworkUserSessionContext.InsertNewUserSessionItems(DBContext, session_obj.SESSION_ITEMS);
            session_obj.SESSION_ITEMS = saved_items.ToList();

            return session_obj;
        }

        public static async Task<IEnumerable<TB_USER_SESSION_ITEM>> InsertNewUserSessionItems(AppDBMainContext DBContext, IEnumerable<TB_USER_SESSION_ITEM>? session_items)
        {
            if (session_items == null) return new List<TB_USER_SESSION_ITEM>();
            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
            session_items = session_items.Select((item) => {
                item.EDIT_TIME = current_time;
                return item;
            });
            
            await DBContext.AddRangeAsync(session_items);
            await DBContext.SaveChangesAsync();

            return session_items;
        }

        public static async Task<TB_USER_SESSION?> GetMostRecentUserSession(AppDBMainContext DBContext) {

            return await DBContext.UserSessions.AsNoTracking().OrderByDescending(s => s.REQUESTED_TIME).FirstOrDefaultAsync();
            /*
            TB_USER_SESSION? most_recent_session_obj;
            var query = DBContext.UserSessions.AsNoTracking().OrderByDescending(s => s.REQUESTED_TIME);
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions() {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                },
                TransactionScopeAsyncFlowOption.Enabled)
            )
            {
                most_recent_session_obj = await query.FirstOrDefaultAsync();
                scope.Complete();
            }
            return most_recent_session_obj;
            */
        }

        // The following fields can be used to query for TB_USER_SESSION records in conjunction: SESSION_ID, CLIENT_IP, THREAD_ID, HOST_IP, IS_VALID
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<IEnumerable<TB_USER_SESSION>> GetUserSessions(AppDBMainContext DBContext, TB_USER_SESSION search_criteria) {
            // Query for sessions:
            IQueryable<TB_USER_SESSION> query = DBContext.UserSessions.AsQueryable();
            if (!string.IsNullOrEmpty(search_criteria.SESSION_ID)) query = query.Where((identity) => identity.SESSION_ID == search_criteria.SESSION_ID);
            if (!string.IsNullOrEmpty(search_criteria.CLIENT_IP)) query = query.Where((identity) => identity.CLIENT_IP == search_criteria.CLIENT_IP);
            if (!string.IsNullOrEmpty(search_criteria.THREAD_ID)) query = query.Where((identity) => identity.THREAD_ID == search_criteria.THREAD_ID);
            if (!string.IsNullOrEmpty(search_criteria.HOST_IP)) query = query.Where((identity) => identity.HOST_IP == search_criteria.HOST_IP);
            query = query.Where((identity) => identity.IS_VALID == search_criteria.IS_VALID);

            // Get all related session items:
            var session_lst = await query.ToListAsync();
            IEnumerable<string?> all_session_ids = session_lst.Select(session => session.SESSION_ID);
            var session_item_lst = await DBContext.UserSessionItems.Where(item => all_session_ids.Contains(item.SESSION_ID)).ToListAsync();

            // Match sessions and their items:
            session_lst = session_lst.Select(session => {
                session.SESSION_ITEMS = session_item_lst.Where(item => item.SESSION_ID == session.SESSION_ID).ToList();
                return session;
            }).ToList();
            
            return session_lst;
        }



        public static async Task InvalidateUserSessions(AppDBMainContext DBContext, string sessionID, string clientIP, string threadID, string hostIP) {
            var session_id_param = new SqlParameter
            {
                ParameterName = "SESSION_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = sessionID
            };

            var client_ip_param = new SqlParameter
            {
                ParameterName = "CLIENT_IP",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = clientIP
            };

            var thread_id_param = new SqlParameter
            {
                ParameterName = "THREAD_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = threadID
            };

            var host_ip_param = new SqlParameter
            {
                ParameterName = "HOST_IP",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = hostIP
            };

            await DBContext.Database.ExecuteSqlRawAsync("EXEC NETWORK.INVALIDATE_USER_SESSIONS @SESSION_ID, @CLIENT_IP, @THREAD_ID, @HOST_IP", new object[] { session_id_param, client_ip_param, thread_id_param, host_ip_param });
        }


    }


}
