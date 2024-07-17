using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models.Mongo
{
    public class O_GOOGLE_TREND_NEWS_IMAGE
    {
        public string? newsUrl { get; set; }
        public string? source { get; set; }
        public string? imageUrl { get; set; }

        [BsonExtraElements]
        public BsonDocument? ExtraElements { get; set; }

    }
}
