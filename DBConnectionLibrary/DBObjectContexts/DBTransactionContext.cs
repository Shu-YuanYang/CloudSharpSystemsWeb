using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts
{
    public class DBTransactionContext
    {

        public static async Task DBTransact(AppDBMainContext DBContext, Func<AppDBMainContext, IDbContextTransaction, Task> procedure) {
            // Reference: https://www.entityframeworktutorial.net/entityframework6/transaction-in-entity-framework.aspx#google_vignette
            using (IDbContextTransaction transaction = DBContext.Database.BeginTransaction())
            {
                try
                {
                    await procedure(DBContext, transaction);
                    //DBContext.SaveChanges();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public static async Task<DateTime> DBGetDateTime(AppDBMainContext DBContext) {

            var datetime = DBContext.SQLParameterType("DATE_TIME", System.Data.DbType.DateTime, null, System.Data.ParameterDirection.Output);

            string SQLCommand = DBContext.FormatSelectCurrentTimestampSQL("@DATE_TIME");
			await DBContext.Database.ExecuteSqlRawAsync(SQLCommand, new object[] { datetime });
            DateTime value = (DateTime) ((IDbDataParameter) datetime).Value!;
            return value;
        }

    }
}
