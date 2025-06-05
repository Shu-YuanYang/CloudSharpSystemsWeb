using DBConnectionLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DBConnectionLibrary.DBObjectContexts.SQL
{
	public class EmailContext
	{
		public enum EmailDBStatus { 
			PENDING, SCHEDULED, SENT, CANCELLED, BLOCKED, FAILED
		}



		public static async Task<List<TB_EMAIL_HEADER>> GetEmails(AppDBMainContext DBContext, TB_EMAIL_HEADER search_criteria)
		{
			IQueryable<TB_EMAIL_HEADER> query = DBContext.EmailHeaders;
			if (String.IsNullOrEmpty(search_criteria.EMAIL_ID)) query = query.Where((email) => email.EMAIL_ID == search_criteria.EMAIL_ID);
			if (String.IsNullOrEmpty(search_criteria.UPLOADED_BY)) query = query.Where((email) => email.UPLOADED_BY == search_criteria.UPLOADED_BY);
			if (String.IsNullOrEmpty(search_criteria.STATUS)) query = query.Where((email) => email.STATUS == search_criteria.STATUS);
			return await query.ToListAsync();
		}
		
		public static async Task<TB_EMAIL_HEADER> InsertEmailSchedule(AppDBMainContext DBContext, TB_EMAIL_HEADER email) {
			DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);
			email.UPLOADED_TIME = current_time;
			email.EDIT_TIME = current_time;
			var insert_result = await DBContext.EmailHeaders.AddAsync(email);
			await DBContext.SaveChangesAsync();
			return insert_result.Entity;
		}

		/*
		public static async Task UpdateEmailStatuses(AppDBMainContext DBContext, ICollection<TB_EMAIL_HEADER> emails) {
			var email_ids = emails.Select(m => m.EMAIL_ID);
			var changedItemQueryables = DBContext.EmailHeaders
				.Where(m => email_ids.Contains(m.EMAIL_ID))
				//.AsQueryable()
				;

			DateTime current_time = await DBTransactionContext.DBGetDateTime(DBContext);

			foreach (var local_item in emails)
            {
				var DBItem = changedItemQueryables.First(i => i.EMAIL_ID == local_item.EMAIL_ID);
				DBItem.SCHEDULED_TIME = local_item.SCHEDULED_TIME;
				DBItem.STATUS = local_item.STATUS;
				DBItem.NOTE = local_item.NOTE;
				DBItem.EDIT_BY = local_item.EDIT_BY;
				DBItem.EDIT_TIME = current_time;
			}

			DBContext.UpdateRange(changedItemQueryables);
			await DBContext.SaveChangesAsync();
        }
		*/
	}
}
