using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq.Dynamic.Core;



namespace DBConnectionLibrary.DBObjectContexts
{
    public class InterfacesMenuContext
    {
        public static async Task<TB_WEBSITE_MENU_HEADER> GetWebsiteMenuHeader(AppDBMainContext DBContext, string user_id, string menu_name, string display_name)
        {
            if (!menu_name.IsNullOrEmpty())
                return await DBContext.WebsiteMenuHeaders.SingleAsync(h => h.USER_ID == user_id && h.MENU_NAME == menu_name);
            return await DBContext.WebsiteMenuHeaders.SingleAsync(h => h.USER_ID == user_id && h.DISPLAY_NAME == display_name);
        }

        // The following fields can be used to query for TB_WEBSITE_MENU_HEADER records in conjunction: HEADER_ID, SITE_ID, PARENT_HEADER_ID, USER_ID, MENU_NAME, DISPLAY_NAME, 
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<IEnumerable<TB_WEBSITE_MENU_HEADER>> GetWebsiteMenuHeaders(AppDBMainContext DBContext, TB_WEBSITE_MENU_HEADER search_criteria)
        {
            var query = DBContext.WebsiteMenuHeaders.AsQueryable();
            if (!string.IsNullOrEmpty(search_criteria.HEADER_ID)) query = query.Where(t => t.HEADER_ID == search_criteria.HEADER_ID);
            if (!string.IsNullOrEmpty(search_criteria.SITE_ID)) query = query.Where(t => t.SITE_ID == search_criteria.SITE_ID);
            if (!string.IsNullOrEmpty(search_criteria.PARENT_HEADER_ID)) query = query.Where(t => t.PARENT_HEADER_ID == search_criteria.PARENT_HEADER_ID);
            if (!string.IsNullOrEmpty(search_criteria.USER_ID)) query = query.Where(t => t.USER_ID == search_criteria.USER_ID);
            if (!string.IsNullOrEmpty(search_criteria.MENU_NAME)) query = query.Where(t => t.MENU_NAME == search_criteria.MENU_NAME);
            if (!string.IsNullOrEmpty(search_criteria.DISPLAY_NAME)) query = query.Where(t => t.DISPLAY_NAME == search_criteria.DISPLAY_NAME);
            query = query.Where(t => t.IS_ENABLED == 'Y');

            return await query.ToListAsync();
        }

        public static async Task<List<T_WEBSITE_MENU_ITEM>> GetWebsiteSubMenuItemsByMenu(AppDBMainContext DBContext, string site_ID, string menu_name, string user_id)
        {
            var lst = await DBContext.GET_SUBMENU_ITEMS_BY_MENU(site_ID, menu_name, user_id).ToListAsync();
            return lst;
        }

        public static async Task<List<T_WEBSITE_MENU_ITEM>> GetWebsiteMenuItemsByMenu(AppDBMainContext DBContext, string site_ID, string menu_name, string user_id)
        {
            var lst = await DBContext.GET_MENU_ITEMS_BY_MENU(site_ID, menu_name, user_id).ToListAsync();
            return lst;
        }

        public static async Task UpdateWebsiteMenuItemsByMenu(AppDBMainContext DBContext, string site_ID, string menu_name, string user_id, List<T_WEBSITE_MENU_ITEM> menu_items) {
            // await this._app_db_main_context.SaveChangesAsync();

            string headerID = (await DBContext.WebsiteMenuHeaders.Where(header => header.SITE_ID == site_ID && header.MENU_NAME == menu_name && header.USER_ID == user_id).Select(header => header.HEADER_ID).SingleAsync())!;

            var menu_item_names = menu_items.Select(i => i.ITEM_NAME);
            var changedItemQueryables = //await
                DBContext.WebsiteMenuItems
                .Where((item) => item.HEADER_ID == headerID)
                //.WhereBulkContains(menu_items, item => item.ITEM_NAME)
                .Where((item) => menu_item_names.Contains(item.ITEM_NAME))
                //.ToListAsync();
                .AsQueryable();
            //.ExecuteUpdateAsync(item => item.SetProperty(p => p.RANKING, 5));

            int db_change_count = await changedItemQueryables.CountAsync();
            if (db_change_count != menu_items.Count()) {
                throw new InvalidDataException("Website menu item update list must only contain items on the server records!");
            }

            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);

            menu_items.ForEach(local_item =>
            {
                string local_item_name = local_item.ITEM_NAME!;
                var DBItem = changedItemQueryables.First(i => i.ITEM_NAME == local_item_name); //Async(); // do not use async action for now.
                DBItem.RANKING = local_item.RANKING;
                if (local_item.ROUTE == "DELETED") // Shu-Yuan Yang 20240712 added disabling logic.
				{
                    DBItem.RANKING = -1;
                    DBItem.IS_ENABLED = 'N';
				}
                DBItem.EDIT_BY = user_id;
                DBItem.EDIT_TIME = current_time;
            });

            // 03/17/2025 switched from bulk update syntax to update range syntax:
            /*
            await DBContext.BulkUpdateAsync(changedItemList, options =>
                options.ColumnInputExpression = item => new { item.RANKING, item.IS_ENABLED, item.EDIT_BY, item.EDIT_TIME }
            );
            */
            DBContext.UpdateRange(changedItemQueryables);
            await DBContext.SaveChangesAsync();
        }


        public static async Task InsertNewMenuItemProcedure(AppDBMainContext DBContext, string menu_header_ID, T_WEBSITE_MENU_ITEM menu_item, string edit_by)
        {
            var menu_header_ID_param = new SqlParameter
            {
                ParameterName = "MENU_HEADER_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = menu_header_ID
            };

            var item_name_param = new SqlParameter
            {
                ParameterName = "ITEM_NAME",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = menu_item.ITEM_NAME
            };

            var item_display_name_param = new SqlParameter
            {
                ParameterName = "ITEM_DISPLAY_NAME",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = menu_item.DISPLAY_NAME
            };

            var route_type_param = new SqlParameter
            {
                ParameterName = "ROUTE_TYPE",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = menu_item.ROUTE_TYPE
            };

            var route_param = new SqlParameter
            {
                ParameterName = "ROUTE",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = menu_item.ROUTE
            };

            var icon_param = new SqlParameter
            {
                ParameterName = "ICON",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = menu_item.ICON
            };

            var edit_by_param = new SqlParameter
            {
                ParameterName = "EDIT_BY",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = edit_by
            };

            var parameters = new System.Data.Common.DbParameter[] { menu_header_ID_param, item_name_param, item_display_name_param, route_type_param, route_param, icon_param, edit_by_param };
            string formatted_sql_str = DBContext.FormatExecSPSQL("INTERFACES.ADD_WEBSITE_MENU_ITEM", parameters);
            await DBContext.Database.ExecuteSqlRawAsync(formatted_sql_str, parameters);
        }





        public static async Task<List<T_WEBSITE_MENU_ITEM>> GetWebsiteSubMenuItemsPartial(AppDBMainContext DBContext, string site_ID, string menu_name, string user_id)
        {
            var args = new List<object>();
            args.Add(1);
            args.Add("DASHBOARD_HOME_PAGE");

            var query = DBContext.GET_SUBMENU_ITEMS_BY_MENU(site_ID, menu_name, user_id);
            query = query.Where("MENU_RANKING > @0 OR ITEM_NAME = @1", args.ToArray());
            query = query.Where("ROUTE_TYPE <> @0", "ASSET");
            var result_lst = await query.ToListAsync();
            return result_lst;
        }
    }
}
