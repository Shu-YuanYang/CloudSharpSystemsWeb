using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
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
            var datetime = new SqlParameter
            {
                ParameterName = "DATE_TIME",
                DbType = System.Data.DbType.DateTime,
                //Size = 100,
                Direction = System.Data.ParameterDirection.Output
            };
            await DBContext.Database.ExecuteSqlRawAsync("SET @DATE_TIME = GETDATE()", new object[] { datetime });
            DateTime value = (DateTime)datetime.Value;
            return value;
        }

    }
}
