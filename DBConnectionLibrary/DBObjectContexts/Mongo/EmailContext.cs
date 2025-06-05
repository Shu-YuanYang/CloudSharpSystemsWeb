using DBConnectionLibrary.Models;
using DBConnectionLibrary.Models.Mongo;
using MongoDB.Bson;
using MongoDB.Driver;

namespace DBConnectionLibrary.DBObjectContexts.Mongo
{
	public class EmailContext
	{
		public static async Task<List<CL_TEMPLATED_EMAIL>> GetTemplatedEmailsByIDs(AppDBMongoContext DBContext, string[] email_ids)
		{
			var id_filter_builder = Builders<CL_TEMPLATED_EMAIL>.Filter.In(m => m._id, email_ids);

			List<CL_TEMPLATED_EMAIL> documents = await DBContext.TemplatedEmails
			.Find(id_filter_builder)
			.ToListAsync();

			return documents;
		}

		public static async Task<CL_TEMPLATED_EMAIL> AddTemplatedEmail(AppDBMongoContext DBContext, CL_TEMPLATED_EMAIL templated_email) {
			await DBContext.TemplatedEmails.InsertOneAsync(templated_email);
			return templated_email;
		}


		public static async Task<bool> UpdateTemplatedEmail(AppDBMongoContext DBContext, CL_TEMPLATED_EMAIL edited_email) {
			// query filter:
			var id_filter = Builders<CL_TEMPLATED_EMAIL>.Filter.Eq(email => email._id, edited_email._id);

			// specify edited fields
			var update = Builders<CL_TEMPLATED_EMAIL>.Update;
			var fields_defs = new List<UpdateDefinition<CL_TEMPLATED_EMAIL>>();

			if (edited_email.metadata != null) fields_defs.Add(update.Set(email => email.metadata, edited_email.metadata));
			if (edited_email.message != null) {
				if (!String.IsNullOrEmpty(edited_email.message.subject)) fields_defs.Add(update.Set(email => email.message!.subject, edited_email.message.subject));
				if (!String.IsNullOrEmpty(edited_email.message.body_template)) fields_defs.Add(update.Set(email => email.message!.body_template, edited_email.message.body_template));
				if (edited_email.message.param_values != null) fields_defs.Add(update.Set(email => email.message!.param_values, edited_email.message.param_values));
			}
			if (edited_email.recipients != null) fields_defs.Add(update.Set(email => email.recipients, edited_email.recipients));

			// update:
			var result = await DBContext.TemplatedEmails.UpdateOneAsync(id_filter, update.Combine(fields_defs));
			if (!result.IsAcknowledged) throw new Exception("UpdateTemplatedEmail failed!");
			if (result.ModifiedCount == 0) return false;

			return true;
		}



	}


}
