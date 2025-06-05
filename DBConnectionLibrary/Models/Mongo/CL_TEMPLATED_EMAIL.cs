using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.Models
{
	public class TEMPLATED_EMAIL_MESSAGE { 
		public string? subject { get; set; }
		public string? body_template { get; set; }
		public Dictionary<string, string[]>? param_values { get; set; }
	}

	public class CL_TEMPLATED_EMAIL
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? _id { get; set; }
		public object? metadata { get; set; }
		public TEMPLATED_EMAIL_MESSAGE? message { get; set; }
		public List<string>? recipients { get; set; }
	}
}
