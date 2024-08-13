


using DBConnectionLibrary.DBObjectContexts;
using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.Parser;
using System.Linq.Expressions;
using static Azure.Core.HttpHeader;

namespace DBConnectionLibrary
{
    public class AppDataContext
    {
        public static async Task<List<TB_APP>> GetAllAppData(AppDBMainContext DBContext)
        {
            return await DBContext.Apps.ToListAsync();

            /*
            var columnValue = new SqlParameter("columnValue", 1);
            var apps = this._app_main_db_context.Database
                .SqlQueryRaw<string>($"SELECT APP_ID FROM APPLICATIONS.TB_APP WHERE 1 = @columnValue", columnValue)
                .ToList();
            
            return apps;*/
        }

        public static async Task<string> GetTestProcedureOutput(AppDBMainContext DBContext)
        {
            var mesg = new SqlParameter
            {
                ParameterName = "MESG",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = "Test"
            };
            
            var id = new SqlParameter
            {
                ParameterName = "ID",
                DbType = System.Data.DbType.String,
                Size = 100,
                Direction = System.Data.ParameterDirection.Output
            };
            await DBContext.Database.ExecuteSqlRawAsync("EXEC [dbo].[PROC_TEST] @MESG, @ID OUTPUT", new object[] { mesg, id });
            string id_str = id.Value.ToString()!;
            return id_str;
        }

        /*
        private static IQueryable<V_APP_DATA_CONTROL> _GetAppDataControlQuery(AppDBMainContext DBContext, string app_ID, string? control_name, string? control_type, string? control_level) 
        {

            if (String.IsNullOrEmpty(app_ID)) {
                throw new AccessViolationException("Access to app data settings must be queried with valid app ID specification.");
            }

            string query_string = "SELECT * FROM APPLICATIONS.V_APP_DATA_CONTROL WHERE APP_ID = @ID AND CONTROL_NAME = @CNAME AND CONTROL_TYPE = @CTYPE AND CONTROL_LEVEL = @CLEVEL AND IS_APP_ENABLED = 'Y' AND IS_CONTROL_ENABLED = 'Y'";

            var param_list = new List<object> { };

            var app_id_param = new SqlParameter { ParameterName = "ID", Value = app_ID, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
            param_list.Add(app_id_param);


            if (String.IsNullOrEmpty(control_name))
            {
                query_string = query_string.Replace("@CNAME", "CONTROL_NAME");
            }
            else
            {
                var control_name_param = new SqlParameter { ParameterName = "CNAME", Value = control_name, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
                param_list.Add(control_name_param);
            }

            if (String.IsNullOrEmpty(control_type))
            {
                query_string = query_string.Replace("@CTYPE", "CONTROL_TYPE");
            }
            else
            {
                var control_type_param = new SqlParameter { ParameterName = "CTYPE", Value = control_type, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
                param_list.Add(control_type_param);
            }

            if (String.IsNullOrEmpty(control_level))
            {
                query_string = query_string.Replace("@CLEVEL", "CONTROL_LEVEL");
            }
            else
            {
                var control_level_param = new SqlParameter { ParameterName = "CLEVEL", Value = control_level, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
                param_list.Add(control_level_param);
            }


            var result_set = DBContext.Database.SqlQueryRaw<V_APP_DATA_CONTROL>(query_string, param_list.ToArray());
            //var result_set = DBContext.Database.SqlQueryRaw<Object>(query_string);
            
            return result_set;
        }

        public static async Task<List<V_APP_DATA_CONTROL> > GetAppDataControlList(AppDBMainContext DBContext, string app_ID, string? control_name, string? control_type, string? control_level)
        {
            var result_set = await _GetAppDataControlQuery(DBContext, app_ID, control_name, control_type, control_level).ToListAsync();
            return result_set;
        }
        */


        public static async Task<string> GetAppDataControlValue(AppDBMainContext DBContext, string app_ID, string? control_name, string? control_type, string? control_level)
        {
            //Object control = await _GetAppDataControlQuery(DBContext, app_ID, control_name, control_type, control_level).SingleAsync();

            string query_string = "SELECT TOP 1 @VAL = CONTROL_VALUE FROM APPLICATIONS.V_APP_DATA_CONTROL WHERE APP_ID = @ID AND CONTROL_NAME = @CNAME AND CONTROL_TYPE = @CTYPE AND CONTROL_LEVEL = @CLEVEL AND IS_APP_ENABLED = 'Y' AND IS_CONTROL_ENABLED = 'Y'";

            var param_list = new List<object> { };

            var app_id_param = new SqlParameter { ParameterName = "ID", Value = app_ID, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
            param_list.Add(app_id_param);

            
            if (String.IsNullOrEmpty(control_name))
            {
                query_string = query_string.Replace("@CNAME", "CONTROL_NAME");
            }
            else
            {
                var control_name_param = new SqlParameter { ParameterName = "CNAME", Value = control_name, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
                param_list.Add(control_name_param);
            }

            if (String.IsNullOrEmpty(control_type))
            {
                query_string = query_string.Replace("@CTYPE", "CONTROL_TYPE");
            }
            else
            {
                var control_type_param = new SqlParameter { ParameterName = "CTYPE", Value = control_type, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
                param_list.Add(control_type_param);
            }

            if (String.IsNullOrEmpty(control_level))
            {
                query_string = query_string.Replace("@CLEVEL", "CONTROL_LEVEL");
            }
            else
            {
                var control_level_param = new SqlParameter { ParameterName = "CLEVEL", Value = control_level, DbType = System.Data.DbType.String, Direction = System.Data.ParameterDirection.Input };
                param_list.Add(control_level_param);
            }
            

            var cname_param = new SqlParameter
            {
                ParameterName = "VAL",
                DbType = System.Data.DbType.String,
                Size = 100,
                Direction = System.Data.ParameterDirection.Output
            };
            
            //param_list.Add(control_name_param);
            //param_list.Add(control_type_param);
            //param_list.Add(control_level_param);
            param_list.Add(cname_param);

            //await DBContext.Database.ExecuteSqlRawAsync(query_string, new object[] { app_id_param, control_name_param, control_type_param, control_level_param, cname_param });
            await DBContext.Database.ExecuteSqlRawAsync(query_string, param_list.ToArray());
            return cname_param.Value.ToString()!;
            
            //return control;
        }



        // The following fields can be used to query for V_APP_DATA_CONTROL records in conjunction: APP_ID, CONTROL_NAME, CONTROL_TYPE, CONTROL_LEVEL, CONTROL_VALUE, CONTROL_NOTE
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<IEnumerable<V_APP_DATA_CONTROL>> GetAppDataControlViews(AppDBMainContext DBContext, V_APP_DATA_CONTROL search_criteria) {
            var query = DBContext.AppDataControlView.AsQueryable();
            if (!string.IsNullOrEmpty(search_criteria.APP_ID)) query = query.Where(t => t.APP_ID == search_criteria.APP_ID);
            if (!string.IsNullOrEmpty(search_criteria.CONTROL_NAME)) query = query.Where(t => t.CONTROL_NAME == search_criteria.CONTROL_NAME);
            if (!string.IsNullOrEmpty(search_criteria.CONTROL_TYPE)) query = query.Where(t => t.CONTROL_TYPE == search_criteria.CONTROL_TYPE);
            if (!string.IsNullOrEmpty(search_criteria.CONTROL_LEVEL)) query = query.Where(t => t.CONTROL_LEVEL == search_criteria.CONTROL_LEVEL);
            if (!string.IsNullOrEmpty(search_criteria.CONTROL_VALUE)) query = query.Where(t => t.CONTROL_VALUE == search_criteria.CONTROL_VALUE);
            if (!string.IsNullOrEmpty(search_criteria.CONTROL_NOTE)) query = query.Where(t => t.CONTROL_NOTE == search_criteria.CONTROL_NOTE);
            query = query.Where(t => t.IS_APP_ENABLED == 'Y');
            query = query.Where(t => t.IS_CONTROL_ENABLED == 'Y');

            return await query.ToListAsync();
        }


        public static async Task<TB_CENTRAL_SYSTEM_LOG> WriteSystemLog(AppDBMainContext DBContext, TB_CENTRAL_SYSTEM_LOG log) {
            log.LOG_ID = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("D");
            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
            log.EDIT_TIME = current_time;

            await DBContext.CentralSystemLogs.AddAsync(log);
            await DBContext.SaveChangesAsync();

            return log;
        }

        public static async Task<List<T_CENTRAL_SYSTEM_LOG_VOLUME> > GetRecentSystemLogVolume(AppDBMainContext DBContext, int dateOffSet, string appID)
        {
            var query = from volume in DBContext.CENTRAL_SYSTEM_LOG_VOLUME(dateOffSet, appID)
                        orderby volume.DATE
                        select volume;

            var lst = await query.ToListAsync();
            return lst;
        }

        // The following fields can be used to query for TB_PROGRAM_STATUS records in conjunction: APP_ID, PROGRAM_TYPE, PROGRAM_STATUS, RESOURCE_SEP, RESOURCE
        // Empty fields in the search_criteria will not be used to query.
        public static async Task<IEnumerable<TB_PROGRAM_STATUS>> GetProgramStatuses(AppDBMainContext DBContext, TB_PROGRAM_STATUS search_criteria)
        {
            var query = DBContext.ProgramStatuses.AsQueryable();
            if (!string.IsNullOrEmpty(search_criteria.APP_ID)) query = query.Where(t => t.APP_ID == search_criteria.APP_ID);
            if (!string.IsNullOrEmpty(search_criteria.PROGRAM_TYPE)) query = query.Where(t => t.PROGRAM_TYPE == search_criteria.PROGRAM_TYPE);
            if (!string.IsNullOrEmpty(search_criteria.PROGRAM_STATUS)) query = query.Where(t => t.PROGRAM_STATUS == search_criteria.PROGRAM_STATUS);
            if (!string.IsNullOrEmpty(search_criteria.RESOURCE_SEP)) query = query.Where(t => t.RESOURCE_SEP == search_criteria.RESOURCE_SEP);
            if (!string.IsNullOrEmpty(search_criteria.RESOURCE)) query = query.Where(t => t.RESOURCE == search_criteria.RESOURCE);

            return await query.ToListAsync();
        }

        public static async Task<TB_PROGRAM_STATUS> UpdateProgramStatus(AppDBMainContext DBContext, TB_PROGRAM_STATUS statusRecord) {
            DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
            statusRecord.EDIT_TIME = current_time;

            // reference on how to use the Update function in EF Core: https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew
            await DBContext.ProgramStatuses
                .Where(rec => rec.PROGRAM_ID == statusRecord.PROGRAM_ID && rec.APP_ID == statusRecord.APP_ID)
                .ExecuteUpdateAsync(s => s.SetProperty(rec => rec.LAST_TRACE_ID, rec => statusRecord.LAST_TRACE_ID)
                    .SetProperty(rec => rec.PROGRAM_STATUS, rec => statusRecord.PROGRAM_STATUS)
                    .SetProperty(rec => rec.LAST_LOG_TIME, rec => statusRecord.LAST_LOG_TIME)
                    .SetProperty(rec => rec.NOTES, rec => statusRecord.NOTES)
                    .SetProperty(rec => rec.EDIT_BY, rec => statusRecord.EDIT_BY)
                    .SetProperty(rec => rec.EDIT_TIME, rec => statusRecord.EDIT_TIME)
                );
            await DBContext.SaveChangesAsync();

            return statusRecord;
        }

        public static async Task UpdateTaskStatuses(AppDBMainContext DBContext, string app_ID, string program_type, string edit_by)
        {
            var app_id_param = new SqlParameter
            {
                ParameterName = "APP_ID",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = app_ID
            };

            var program_type_param = new SqlParameter
            {
                ParameterName = "PROGRAM_TYPE",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = program_type
            };

            var edit_by_param = new SqlParameter
            {
                ParameterName = "EDIT_BY",
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input,
                Value = edit_by
            };

            var parameters = new object[] { app_id_param, program_type_param, edit_by_param };
            await DBContext.Database.ExecuteSqlRawAsync("EXEC APPLICATIONS.UPDATE_TASK_STATUSES @APP_ID, @PROGRAM_TYPE, @EDIT_BY", parameters);
        }


        public static IEnumerable<string> GetQueryStr(AppDBMainContext DBContext, string queryID, QueryList? queryList)
        {
            var result_lst = QueryListBuilder.WhereStr(queryList!, DBContext.Validator[queryID]);
            return result_lst;
        }


    }




}