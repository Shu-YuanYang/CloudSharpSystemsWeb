using DBConnectionLibrary.Models.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static MongoDB.Driver.WriteConcern;

namespace DBConnectionLibrary.DBObjectContexts.Mongo
{
    public class TeamNoteContext
    {

        public static async Task<IEnumerable<CL_TEAM_NOTE>> GetTeamNotesByTeams(AppDBMongoContext DBContext, IEnumerable<string> teamIDs) {
            var team_notes = await DBContext.TeamNotes.Find(note => teamIDs.Contains(note.TEAM_ID)).ToListAsync();
            return team_notes;
        }


        // The following fields can be used to query the note records: _id, TEAM_ID, SENDER_ID, PRIORITY, STATUS
        // Empty fields will not be used to query.
        public static async Task<IEnumerable<CL_TEAM_NOTE>> GetTeamNotes(AppDBMongoContext DBContext, CL_TEAM_NOTE search_criteria) {
            //IQueryable<CL_TEAM_NOTE> query = DBContext.TeamNotes.AsQueryable();
            List<FilterDefinition<CL_TEAM_NOTE>> filters = new List<FilterDefinition<CL_TEAM_NOTE>>();
            
            if (!String.IsNullOrEmpty(search_criteria._id)) filters.Add(Builders<CL_TEAM_NOTE>.Filter.Eq(note => note._id, search_criteria._id));
            if (!String.IsNullOrEmpty(search_criteria.TEAM_ID)) filters.Add(Builders<CL_TEAM_NOTE>.Filter.Eq(note => note.TEAM_ID, search_criteria.TEAM_ID));
            if (!String.IsNullOrEmpty(search_criteria.SENDER_ID)) filters.Add(Builders<CL_TEAM_NOTE>.Filter.Eq(note => note.SENDER_ID, search_criteria.SENDER_ID));
            if (!String.IsNullOrEmpty(search_criteria.TITLE)) filters.Add(Builders<CL_TEAM_NOTE>.Filter.Eq(note => note.TITLE, search_criteria.TITLE));
            if (!String.IsNullOrEmpty(search_criteria.PRIORITY)) filters.Add(Builders<CL_TEAM_NOTE>.Filter.Eq(note => note.PRIORITY, search_criteria.PRIORITY));
            if (!String.IsNullOrEmpty(search_criteria.STATUS)) filters.Add(Builders<CL_TEAM_NOTE>.Filter.Eq(note => note.STATUS, search_criteria.STATUS));
            var filter = Builders<CL_TEAM_NOTE>.Filter.And(filters);
            var query = await DBContext.TeamNotes.FindAsync(filter);
            return await query.ToListAsync();
        }



        public static async Task<CL_TEAM_NOTE> AddTeamNote(AppDBMongoContext DBContext, CL_TEAM_NOTE note) {
            await DBContext.TeamNotes.InsertOneAsync(note);
            return note;
        }

        // Editable fields in CL_TEAM_NOTE: PRIORITY, MESSAGE, NOTE_HASH, STATUS, EDIT_BY, EDIT_TIME,
        public static async Task<bool> UpdateTeamNote(AppDBMongoContext DBContext, CL_TEAM_NOTE edited_note, string old_note_hash) {
            
            // query filter
            var id_filter = Builders<CL_TEAM_NOTE>.Filter.Eq(note => note._id, edited_note._id);
            var hash_filter = Builders<CL_TEAM_NOTE>.Filter.Eq(note => note.NOTE_HASH, old_note_hash);
            var filter = Builders<CL_TEAM_NOTE>.Filter.And(id_filter, hash_filter);

            // specify edited fields
            var update = Builders<CL_TEAM_NOTE>.Update;
            var fields_defs = new List<UpdateDefinition<CL_TEAM_NOTE>>();

            if (!String.IsNullOrEmpty(edited_note.PRIORITY)) fields_defs.Add(update.Set(note => note.PRIORITY, edited_note.PRIORITY));
            if (!String.IsNullOrEmpty(edited_note.TITLE)) fields_defs.Add(update.Set(note => note.TITLE, edited_note.TITLE));
            if (!String.IsNullOrEmpty(edited_note.MESSAGE)) fields_defs.Add(update.Set(note => note.MESSAGE, edited_note.MESSAGE));
            if (!String.IsNullOrEmpty(edited_note.NOTE_HASH)) fields_defs.Add(update.Set(note => note.NOTE_HASH, edited_note.NOTE_HASH));
            //if (!String.IsNullOrEmpty(edited_note.PERMISSIONS))
            if (!String.IsNullOrEmpty(edited_note.STATUS)) fields_defs.Add(update.Set(note => note.STATUS, edited_note.STATUS));
            if (!String.IsNullOrEmpty(edited_note.EDIT_BY)) fields_defs.Add(update.Set(note => note.EDIT_BY, edited_note.EDIT_BY));
            fields_defs.Add(update.Set(note => note.EDIT_TIME, edited_note.EDIT_TIME));

            // update:
            var result = await DBContext.TeamNotes.UpdateOneAsync(filter, update.Combine(fields_defs));
            if (!result.IsAcknowledged) throw new Exception("UpdateTeamNote failed!");
            if (result.ModifiedCount == 0) return false;

            return true;
        }


        // Move completed or removed team notes to the log if they have been aging for more than age_in_hours:
        public static async Task ClearOldTeamNotes(AppDBMongoContext DBContext, int age_in_hours, string program_name) {
            DateTime threshold_time = DateTime.Now.AddHours(-age_in_hours);

            var status_filter = new BsonDocument("$not", new BsonDocument("$eq", "TODO"));
            var age_filter = new BsonDocument("$lt", threshold_time); //Builders<CL_TEAM_NOTE>.Filter.Lt(note => note.EDIT_TIME, threshold_time);
            var matchDocument = new BsonDocument("$and", new BsonArray {
                new BsonDocument("STATUS", status_filter),
                new BsonDocument("EDIT_TIME", age_filter)
            });
            var addFieldDocument = new BsonDocument {
                { "LAST_EDIT_BY", "$EDIT_BY" },
                { "LAST_EDIT_TIME", "$EDIT_TIME" },
                { "DELETED_BY", program_name },
                { "DELETED_TIME", DateTime.Now }
            };
            var projectDocument = new BsonDocument { 
                { "EDIT_BY", 0 }, 
                { "EDIT_TIME", 0}
            };

            var resultCursor = await DBContext.TeamNotes.Aggregate()
                .Match(matchDocument)
                .AppendStage<BsonDocument>(new BsonDocument("$addFields", addFieldDocument))
                .Project(projectDocument)
                .MergeAsync(DBContext.TeamNoteLogs);

            /*
            BsonDocument[] pipeline = new BsonDocument[] {
                new BsonDocument("$match", matchDocument),
                new BsonDocument("$addFields", addFieldDocument),
                new BsonDocument("$project", projectDocument),
                new BsonDocument("$merge", "CL_TEAM_NOTE_LOG")
            };

            //var result = await DBContext.TeamNotes.AggregateAsync<BsonDocument>(pipeline);
            */

            var del_status_filter = Builders<CL_TEAM_NOTE>.Filter.Ne(note => note.STATUS, "TODO");
            var del_age_filter = Builders<CL_TEAM_NOTE>.Filter.Lt(note => note.EDIT_TIME, threshold_time);
            var del_filter = Builders<CL_TEAM_NOTE>.Filter.And(del_status_filter, del_age_filter);
            await DBContext.TeamNotes.DeleteManyAsync(del_filter);
            
        }

    }


}
