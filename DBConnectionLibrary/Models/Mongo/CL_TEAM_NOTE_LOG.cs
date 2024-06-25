using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace DBConnectionLibrary.Models.Mongo
{
    public class CL_TEAM_NOTE_LOG : TEAM_NOTE_BASE
    {
        public string? LAST_EDIT_BY { get; set; }
        public DateTime LAST_EDIT_TIME { get; set; }
        public string? DELETED_BY { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime DELETED_TIME { get; set; }
    }

}
