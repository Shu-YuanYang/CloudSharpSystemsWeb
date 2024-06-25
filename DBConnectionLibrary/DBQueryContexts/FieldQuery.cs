using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBQueryContexts
{


    public class QueryComparisonOperator {
        public string? functional_operator { get; set; }
        private Func<string, string, string, string>? __function { get; set; }
        public Func<string, string, string> function {
            get => (field_name, param_symbol) => __function!(field_name, functional_operator!, param_symbol);
        }

        QueryComparisonOperator(string? functional_operator, Func<string, string, string, string>? function)
        {
            this.functional_operator = functional_operator;
            this.__function = function;
        }




        private static readonly Func<string, string, string, string> LogicalComparisonFunction = (field_name, functional_operator, param_symbol) => $"{field_name} {functional_operator} {param_symbol}";
        private static readonly Func<string, string, string, string> AttributeComparisonFunction = (field_name, functional_operator, param_symbol) => $"{field_name}.{functional_operator}({param_symbol})";


        
        public static readonly string OPER_EQUAL_TO = "=";
        public static readonly string OPER_NOT_EQUAL_TO = "<>";
        public static readonly string OPER_LESS_THAN = "<";
        public static readonly string OPER_GREATER_THAN = ">";
        public static readonly string OPER_CONTAINS = "Contains";
        public static readonly Dictionary<string, QueryComparisonOperator> QueryComparisonOperatorDict = new Dictionary<string, QueryComparisonOperator>
        {
            { OPER_EQUAL_TO, new QueryComparisonOperator(OPER_EQUAL_TO, QueryComparisonOperator.LogicalComparisonFunction) },
            { OPER_NOT_EQUAL_TO, new QueryComparisonOperator(OPER_NOT_EQUAL_TO, QueryComparisonOperator.LogicalComparisonFunction) },
            { OPER_LESS_THAN, new QueryComparisonOperator(OPER_LESS_THAN, QueryComparisonOperator.LogicalComparisonFunction) },
            { OPER_GREATER_THAN, new QueryComparisonOperator(OPER_GREATER_THAN, QueryComparisonOperator.LogicalComparisonFunction) },
            { OPER_CONTAINS, new QueryComparisonOperator(OPER_CONTAINS, QueryComparisonOperator.AttributeComparisonFunction) }
        };
    }

    

    public enum QueryLogicOperator
    {
        AND, OR
    }

    public class FieldQuery {
        public string? field_name { get; set; }
        public TypeCode? field_type { get; set; }
        public string? compared_value_raw { get; set; }
        public object? compared_value { get; set; }
        public string? comparison_operator { get; set; }
    }





    



    public class QueryList
    {
        public string? logic_operator { get; set; }
        public FieldQuery[]? field_queries { get; set; }
        public QueryList[]? sub_query_lists { get; set; }
    };
}
