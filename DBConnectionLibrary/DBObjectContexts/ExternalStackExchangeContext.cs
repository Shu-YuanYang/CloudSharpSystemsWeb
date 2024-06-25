using AuxiliaryClassLibrary.Functions;
using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class ExternalStackExchangeContext
    {

        public static async Task<List<TB_CS_EDUCATOR_POSTS>> GetCSEducatorPostsByQuery(AppDBMainContext DBContext, QueryList? queryList)
        {
            if (queryList == null) queryList = new QueryList { logic_operator = QueryLogicOperator.OR.ToString() };
            var query = DBContext.ExternalStackExchangeCsEducatorPosts.AsQueryable()
            // Dynamic query goes here
                .Where(queryList, DBContext.Validator[nameof(GetCSEducatorPostsByQuery)])
                .OrderByDescending(post => post.VIEW_COUNT).Take(50);
            var result_lst = await query.ToListAsync();
            return result_lst;
            
            /*
            var query = DBContext.GET_SUBMENU_ITEMS_BY_MENU(site_ID, menu_name, user_id);
            query = query.Where("MENU_RANKING > @0 OR ITEM_NAME = @1", args.ToArray());
            query = query.Where("ROUTE_TYPE <> @0", "ASSET");
            var result_lst = await query.ToListAsync();
            return result_lst;
            */
        }

        

    }
}
