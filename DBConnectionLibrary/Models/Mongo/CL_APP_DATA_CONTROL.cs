using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models.Mongo
{
    public class CL_APP_DATA_CONTROL
    {
        //public const string TEAM_NOTE_PRIORITY = "TEAM_NOTE_PRIORITY";
        public string? _id { get; set; }


        [BsonExtraElements]
        public BsonDocument? ExtraElements { get; set; }

    }
}
