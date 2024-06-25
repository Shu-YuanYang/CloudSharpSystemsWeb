using DBConnectionLibrary.Models.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBObjectContexts.Mongo
{
    public class MongoAppDataContext
    {
        private static async Task<IDictionary<string, object>> GetDataDocumentDict(AppDBMongoContext DBContext, string documentID) {
            var query = await DBContext.AppDataControl.FindAsync(doc => doc._id == documentID);
            var doc = await query.FirstAsync();
            BsonDocument field_map = doc.ExtraElements!.AsBsonDocument;
            var field_dict = field_map.ToDictionary();
            return field_dict;
        }

        public static async Task<IDictionary<string, int>> GetTeamNotePriorityMap(AppDBMongoContext DBContext)
        {
            var priority_dict = await MongoAppDataContext.GetDataDocumentDict(DBContext, "TEAM_NOTE_PRIORITY");
            var parsed_priority_dict = priority_dict.ToDictionary(kvp => kvp.Key, kvp => (int)kvp.Value);
            return parsed_priority_dict;
        }

        public static async Task<IDictionary<string, string>> GetTeamNoteStatusMap(AppDBMongoContext DBContext)
        {
            var status_dict = await MongoAppDataContext.GetDataDocumentDict(DBContext, "TEAM_NOTE_STATUS");
            var parsed_status_dict = status_dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()!);
            return parsed_status_dict;
        }

        public static async Task<IDictionary<string, string>> GetTeamNotePermissionMap(AppDBMongoContext DBContext)
        {
            var status_dict = await MongoAppDataContext.GetDataDocumentDict(DBContext, "TEAM_NOTE_PERMISSION");
            var parsed_status_dict = status_dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()!);
            return parsed_status_dict;
        }

    }
}
