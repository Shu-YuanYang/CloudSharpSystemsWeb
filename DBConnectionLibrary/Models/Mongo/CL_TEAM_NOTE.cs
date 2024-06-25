using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace DBConnectionLibrary.Models.Mongo
{
    public class CL_TEAM_NOTE : TEAM_NOTE_BASE
    {
        /*
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }
        public string? TEAM_ID { get; set; }
        public string? SENDER_ID { get; set; }
        public string? PRIORITY { get; set; }
        public string? TITLE { get; set; }
        public string? MESSAGE { get; set; }*/
        public string? EDIT_BY { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EDIT_TIME { get; set; }
        /*public string? NOTE_HASH { get; set; }
        public O_TEAM_NOTE_PERMISSIONS? PERMISSIONS { get; set; }
        public string? STATUS { get; set; }*/
    }

}
