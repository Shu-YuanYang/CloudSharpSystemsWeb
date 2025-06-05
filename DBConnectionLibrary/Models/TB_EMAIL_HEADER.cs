using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
	[Table(nameof(TB_EMAIL_HEADER), Schema = DB_SCHEMA.COMMUNICATIONS)]
	public class TB_EMAIL_HEADER
	{
		[Key]
		public string? EMAIL_ID { get; set; }
		public string? UPLOADED_BY { get; set; }
		public DateTime UPLOADED_TIME { 
			get => _uploaded_time; 
			set => _uploaded_time = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToUniversalTime(); 
		}
		public DateTime SCHEDULED_TIME { 
			get => _scheduled_time; 
			set => _scheduled_time = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToUniversalTime(); 
		}
		public string? NOTE { get; set; }
		public string? STATUS { get; set; }
		public string? EDIT_BY { get; set; }
		public DateTime EDIT_TIME { 
			get => _edit_time; 
			set => _edit_time = DateTime.SpecifyKind(value, DateTimeKind.Utc).ToUniversalTime(); 
		}

		[NotMapped]
		private DateTime _uploaded_time;
		[NotMapped]
		private DateTime _scheduled_time;
		[NotMapped]
		private DateTime _edit_time;
	}
}
