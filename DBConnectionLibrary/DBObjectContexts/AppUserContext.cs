using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class AppUserContext
    {
        // The following fields can be used to query for TB_APP_USER_IDENTITY records in conjunction: IDENTITY_PROVIDER, USERNAME, USERID, USERNAME_ALIAS, LANGUAGE_CODE
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<List<TB_APP_USER_IDENTITY>> GetUserIdentities(AppDBMainContext DBContext, TB_APP_USER_IDENTITY search_criteria)
        {
            IQueryable<TB_APP_USER_IDENTITY> query = DBContext.AppUserIdentities.AsQueryable();
            if (!string.IsNullOrEmpty(search_criteria.IDENTITY_PROVIDER)) query = query.Where((identity) => identity.IDENTITY_PROVIDER == search_criteria.IDENTITY_PROVIDER);
            if (!string.IsNullOrEmpty(search_criteria.USERNAME)) query = query.Where((identity) => identity.USERNAME == search_criteria.USERNAME);
            if (!string.IsNullOrEmpty(search_criteria.USERID)) query = query.Where((identity) => identity.USERID == search_criteria.USERID);
            if (!string.IsNullOrEmpty(search_criteria.USERNAME_ALIAS)) query = query.Where((identity) => identity.USERNAME_ALIAS == search_criteria.USERNAME_ALIAS);
            if (!string.IsNullOrEmpty(search_criteria.LANGUAGE_CODE)) query = query.Where((identity) => identity.LANGUAGE_CODE == search_criteria.LANGUAGE_CODE);
            query = query.Where((identity) => identity.IS_ENABLED == 'Y');

            return await query.ToListAsync();
        }


        public static async Task<T_APP_IDENTITY_USER_PROFILE_HEADER> GetUserIdentityProfileHeader(AppDBMainContext DBContext, string IdentityProvider, string UserID) {

            var query = from profile_header in DBContext.GET_APP_IDENTITY_USER_PROFILE_HEADER(IdentityProvider, UserID)
                        // where
                        select profile_header;

            var profile_head_result = await query.FirstAsync();
            return profile_head_result;
        }

        public static async Task<IEnumerable<T_APP_IDENTITY_USER_PROFILE_HEADER>> GetUserIdentityProfileHeadersByTeam(AppDBMainContext DBContext, string IdentityProvider, string TeamID) {
            var query = from profile_header in DBContext.GET_APP_IDENTITY_USER_PROFILE_HEADERS_BY_TEAM(IdentityProvider, TeamID)
                        select profile_header;

            var profile_head_results = await query.ToListAsync();
            return profile_head_results;
        }

        public static async Task<IEnumerable<TB_APP_USER>> GetUsersByIDs(AppDBMainContext DBContext, IEnumerable<string> userIDs) {
            var query = DBContext.AppUsers.Where(u => userIDs.Contains(u.USERID));
            return await query.ToListAsync();
        }


        public static async Task<List<TB_CENTRAL_SYSTEM_LOG>> GetAppUserActivitiesByQuery(AppDBMainContext DBContext, QueryList? queryList)
        {
            if (queryList == null) queryList = new QueryList { logic_operator = QueryLogicOperator.OR.ToString() };
            var query = DBContext.CentralSystemLogs.AsQueryable()
                .Where(log => log.RECORD_VALUE3 != null && log.RECORD_VALUE3.Contains("Identity Username"))
                // Dynamic query goes here
                .Where(queryList, DBContext.Validator[nameof(GetAppUserActivitiesByQuery)])
                .OrderByDescending(log => log.EDIT_TIME).Take(50);
            var result_lst = await query.ToListAsync();
            return result_lst;
        }
    }
}
