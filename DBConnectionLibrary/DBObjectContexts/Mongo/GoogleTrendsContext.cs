using DBConnectionLibrary.Models.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts.Mongo
{
    public class GoogleTrendsContext
    {
        public static async Task<CL_GOOGLE_DAILY_TREND> GetLatestTrendSearches(AppDBMongoContext DBContext)
        {
            DateTime threshold_time = DateTime.UtcNow.AddDays(-4); // limit search ranges to 4 days

            var time_filter = new BsonDocument("$gt", threshold_time);
            var matchDocument = new BsonDocument("FEED_TIME", time_filter); 

            var trends = await DBContext.GoogleDailyTrendsCollection.Aggregate()
                .Match(matchDocument)
                //.Project(projectDocument)
                .SortByDescending(doc => doc.FEED_TIME)
                .FirstAsync();
            return trends;
        }


    }


}
