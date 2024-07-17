using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models.Mongo
{
    public class O_GOOGLE_TREND_ARTICLE
    {
        public string? title { get; set; }
        public string? timeAgo { get; set; }
        public string? source { get; set; }
        public O_GOOGLE_TREND_NEWS_IMAGE? image { get; set; }
        public string? url { get; set; }
        public string? snippet { get; set; }

        [BsonExtraElements]
        public BsonDocument? ExtraElements { get; set; }
    }
}
