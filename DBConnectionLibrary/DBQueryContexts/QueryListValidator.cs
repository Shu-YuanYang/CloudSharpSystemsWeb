using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace DBConnectionLibrary.DBQueryContexts
{
    public class QueryListValidator
    {
        private readonly DynamicQueryConfig __dynamic_qeury_config;
        public QueryListValidator(DynamicQueryConfig dynamic_query_config) 
        { 
            this.__dynamic_qeury_config = dynamic_query_config;
        }

        public FieldQueryConfig[] this[string key]
        {
            get => this.__dynamic_qeury_config.FieldQueryConfigDict![key];
        }


        private QueryableFieldOperation ComputeValidFieldOperation(FieldQueryConfig fieldConfig) {
            var comp_operators = QueryComparisonOperator.QueryComparisonOperatorDict.Select(comp_operator_tup => comp_operator_tup.Value.functional_operator!);
            
            // disallow .Contains(@param) comparison if data type is not string
            if (!fieldConfig.FieldType!.Equals(Enum.GetName(TypeCode.String))) comp_operators = comp_operators.Where(comp_operator => !comp_operator.Equals(QueryComparisonOperator.OPER_CONTAINS));  
            
            return new QueryableFieldOperation { field_name = fieldConfig.QueryableField, comparison_operators = comp_operators };
        }

        public QueryableOperations GetQueryableOperations(string queryID)
        {
            QueryableOperations queryableOperations = new QueryableOperations();

            queryableOperations.logical_operators = Enum.GetNames(typeof(QueryLogicOperator)); ;
            
            var field_operations = this[queryID]
                .Select(field_config => ComputeValidFieldOperation(field_config))
                .OrderBy(field_operation => field_operation.field_name);
            queryableOperations.queryable_field_operations = field_operations;

            return queryableOperations;
        }


        public static void ValidateQueryableFields(QueryList query_lst, FieldQueryConfig[] queryable_fields_config ) {
            var valid_logic_operator = (QueryLogicOperator) Enum.Parse(typeof(QueryLogicOperator), query_lst.logic_operator!, true);

            if (/*query_lst.sub_query_lists != null*/!query_lst.sub_query_lists.IsNullOrEmpty()) {
                for (int i = 0; i < query_lst.sub_query_lists!.Length; ++i) ValidateQueryableFields(query_lst.sub_query_lists![i], queryable_fields_config);
            }
            if (/*query_lst.field_queries == null*/query_lst.field_queries.IsNullOrEmpty()) return;
            for (int i = 0; i < query_lst.field_queries!.Length; ++i) {
                var match_lst = queryable_fields_config.Where(conf => conf.QueryableField == query_lst.field_queries![i].field_name);
                if (!match_lst.Any()) 
                    throw new Exception($"Illegal query on field {query_lst.field_queries![i].field_name}!");
                
                var config = match_lst.First();

                if (!QueryComparisonOperator.QueryComparisonOperatorDict.ContainsKey(query_lst.field_queries![i].comparison_operator!))
                    throw new Exception($"Illegal query with operator {query_lst.field_queries![i].comparison_operator}!");

                query_lst.field_queries![i].field_type = (TypeCode) Enum.Parse(typeof(TypeCode), config.FieldType!, true);
                //TypeCode typeCode = query_lst.field_queries[i].field_type!.Value;
                query_lst.field_queries[i].compared_value = Convert.ChangeType(query_lst.field_queries[i].compared_value_raw, query_lst.field_queries[i].field_type!.Value);
                //string type_name = changed_post_ID.GetType().Name;
            }
        }

    }
}
