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
    public class AppTeamContext
    {
        // The following fields can be used to query for TB_APP_USER_IDENTITY records in conjunction: IDENTITY_PROVIDER, USERNAME, USERID, USERNAME_ALIAS, LANGUAGE_CODE
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<IEnumerable<TB_APP_TEAM> > GetTeamsByUser(AppDBMainContext DBContext, string appID, string userID, bool with_default_account_space)
        {
            var query = from team in DBContext.GET_TEAMS_BY_USER(appID, userID, with_default_account_space ? 'Y' : 'N')
                        select team;

            return await query.ToListAsync();
        }

        // The following fields can be used to query for TB_APP_TEAM records in conjunction: TEAM_ID, APP_ID, TEAM_NAME, PRIMARY_CONTACT, OWNED_BY
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<IEnumerable<TB_APP_TEAM>> GetTeams(AppDBMainContext DBContext, TB_APP_TEAM search_criteria) {
            var query = DBContext.AppTeams.AsQueryable();
            if (!string.IsNullOrEmpty(search_criteria.TEAM_ID)) query = query.Where(t => t.TEAM_ID == search_criteria.TEAM_ID);
            if (!string.IsNullOrEmpty(search_criteria.APP_ID)) query = query.Where(t => t.APP_ID == search_criteria.APP_ID);
            if (!string.IsNullOrEmpty(search_criteria.TEAM_NAME)) query = query.Where(t => t.TEAM_NAME == search_criteria.TEAM_NAME);
            if (!string.IsNullOrEmpty(search_criteria.PRIMARY_CONTACT)) query = query.Where(t => t.PRIMARY_CONTACT == search_criteria.PRIMARY_CONTACT);
            if (!string.IsNullOrEmpty(search_criteria.OWNED_BY)) query = query.Where(t => t.OWNED_BY == search_criteria.OWNED_BY);
            query = query.Where(t => t.IS_ENABLED == 'Y');

            return await query.ToListAsync();
        }


    }
}
