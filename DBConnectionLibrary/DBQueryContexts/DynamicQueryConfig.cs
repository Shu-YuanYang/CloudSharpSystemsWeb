using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBQueryContexts
{

    public class FieldQueryConfig
    {
        public string? QueryableField { get; set; }
        public string? FieldType { get; set; }
    }

    public class DynamicQueryConfig
    {
        public Dictionary<string, FieldQueryConfig[]>? FieldQueryConfigDict { get; set; }

        
    }




    public class QueryableFieldOperation { 
        public string? field_name { get; set; }
        public IEnumerable<string>? comparison_operators { get; set; }
    }

    public class QueryableOperations
    {
        public string[]? logical_operators { get; set; }
        public IEnumerable<QueryableFieldOperation>? queryable_field_operations { get; set; }
    }

}
