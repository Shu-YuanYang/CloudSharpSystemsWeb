using DBConnectionLibrary.DBQueryContexts;
using DBConnectionLibrary.Models.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DBConnectionLibrary
{
    public class AppDBMongoContext
    {
        private readonly IMongoDatabase _cloudsharp_userdoc_db;

        public readonly IMongoCollection<CL_APP_DATA_CONTROL> AppDataControl;

        public readonly IMongoCollection<CL_TEAM_NOTE> TeamNotes;
        public readonly IMongoCollection<CL_TEAM_NOTE_LOG> TeamNoteLogs;
        


        public AppDBMongoContext(string connection_string)
        {
            //var mongoClient = new MongoClient(connection_string);
            
            var settings = MongoClientSettings.FromConnectionString(connection_string);
            // Sets the ServerApi field of the settings object to Stable API version 1
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            // Creates a new client and connects to the server
            var mongoClient = new MongoClient(settings);

            this._cloudsharp_userdoc_db = mongoClient.GetDatabase(DB_DATABASE.CLOUDSHARP_USERDOC);

            this.AppDataControl = this._cloudsharp_userdoc_db.GetCollection<CL_APP_DATA_CONTROL>(typeof(CL_APP_DATA_CONTROL).Name);
            this.TeamNotes = this._cloudsharp_userdoc_db.GetCollection<CL_TEAM_NOTE>(typeof(CL_TEAM_NOTE).Name);
            this.TeamNoteLogs = this._cloudsharp_userdoc_db.GetCollection<CL_TEAM_NOTE_LOG>(typeof(CL_TEAM_NOTE_LOG).Name);
        }

        public Object PingDatabases() {
            var cloudsharp_userdoc_result = this._cloudsharp_userdoc_db.RunCommand<BsonDocument>(new BsonDocument("ping", 1));

            return new BsonDocument {
                { DB_DATABASE.CLOUDSHARP_USERDOC, cloudsharp_userdoc_result }
            };
        }



    }
}
