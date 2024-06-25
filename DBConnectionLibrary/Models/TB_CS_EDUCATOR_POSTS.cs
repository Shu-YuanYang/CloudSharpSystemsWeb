using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace DBConnectionLibrary.Models
{
    [Table("TB_CS_EDUCATOR_POSTS", Schema = DB_SCHEMA.EXTERNAL_STACKEXCHANGE)]
    public class TB_CS_EDUCATOR_POSTS
    {
        [Key]
        public string? ID { get; set; }
        public string? POST_TYPE_ID { get; set; }
        [Column(TypeName = "datetime")] // IMPORTANT: EF Core will parse query strings for DateTime as datetime2, so force the column type mapping to datetime!
        public DateTime CREATION_DATE  { get; set; }
	    public int SCORE { get; set; }
        public int VIEW_COUNT { get; set; }
        public string? BODY { get; set; }
        public string? OWNER_USER_ID { get; set; }
        public string? LAST_EDITOR_USER_ID { get; set; }
        [Column(TypeName = "datetime")] // IMPORTANT: EF Core will parse query strings for DateTime as datetime2, so force the column type mapping to datetime!
        public DateTime LAST_EDIT_DATE { get; set; }
        [Column(TypeName = "datetime")] // IMPORTANT: EF Core will parse query strings for DateTime as datetime2, so force the column type mapping to datetime!
        public DateTime LAST_ACTIVITY_DATE { get; set; }
        public string? TITLE { get; set; }
        public string? TAGS { get; set; }
        public int ANSWER_COUNT { get; set; } 
	    public int COMMENT_COUNT { get; set; }
        public string? CONTENT_LICENSE { get; set; }
    }
}
