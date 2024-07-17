using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models.Mongo
{
    public class __O_GOOGLE_TREND_SEARCH_QUERY
    {
        public string? query { get; set; }
        public string? exploreLink { get; set; }
        [BsonExtraElements]
        public BsonDocument? ExtraElements { get; set; }
    }


    public class O_GOOGLE_TREND_SEARCH
    {
        public __O_GOOGLE_TREND_SEARCH_QUERY? title { get; set; }
        public string? formattedTraffic { get; set; }
        public IEnumerable<__O_GOOGLE_TREND_SEARCH_QUERY>? relatedQueries { get; set; }
        public O_GOOGLE_TREND_NEWS_IMAGE? image { get; set; }
        public IEnumerable<O_GOOGLE_TREND_ARTICLE>? articles { get; set; }
        public string? shareUrl { get; set; }
        [BsonExtraElements]
        public BsonDocument? ExtraElements { get; set; }

    }

    public class CL_GOOGLE_DAILY_TREND
    {
        [BsonId]
        public string? _id { get; set; }
        public IEnumerable<O_GOOGLE_TREND_SEARCH>? trendingSearches { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime FEED_TIME { get; set; }

    }
}
