using AuxiliaryClassLibrary.DateTimeHelper;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary.DBQueryContexts
{


    public static class QueryListBuilder
    {
        private class __QueryStr__
        {
            public string? query_str { get; set; }
            public List<object>? query_params { get; set; }
        }



        public static IQueryable<TSource> Where<TSource>(this IQueryable<TSource> source, QueryList queryList, FieldQueryConfig[]? validatorArray = null)
        {
            if (validatorArray.IsNullOrEmpty()) 
            {
                throw new Exception("Dynamic query must have a validator list in the current implementation!");
            }

            QueryListValidator.ValidateQueryableFields(queryList, validatorArray!);

            List<__QueryStr__> query_str_obj_lst = QueryListBuilder.BuildQueryStrings(queryList);
            foreach (var query_str_obj in query_str_obj_lst) source = source.Where(query_str_obj.query_str!, query_str_obj.query_params!.ToArray());
            
            return source;
        }

        public static IEnumerable<string> WhereStr(QueryList queryList, FieldQueryConfig[]? validatorArray = null) {
            if (validatorArray.IsNullOrEmpty())
            {
                throw new Exception("Dynamic query must have a validator list in the current implementation!");
            }

            QueryListValidator.ValidateQueryableFields(queryList, validatorArray!);

            List<__QueryStr__> query_str_obj_lst = QueryListBuilder.BuildQueryStrings(queryList);
            var result_str_lst = query_str_obj_lst.Select(str_obj => {
                var query_param_arr = str_obj.query_params!.ToArray();
                string parsed_query_str = str_obj.query_str!;
                for (int i = 0; i < query_param_arr.Length; ++i) 
                {
                    if (Convert.GetTypeCode(query_param_arr[i]) == TypeCode.String)
                    {
                        string param_str = query_param_arr[i].ToString()!.Trim();
                        param_str = param_str.Replace("'", "''");
                        parsed_query_str = parsed_query_str.Replace($"@{i}", $"'{param_str}'");
                    }
                    else if (Convert.GetTypeCode(query_param_arr[i]) == TypeCode.DateTime) 
                    {
                        string param_str = TimestampHelper.ToUniversalISOFormatString((DateTime) query_param_arr[i]);
                        parsed_query_str = parsed_query_str.Replace($"@{i}", $"'{param_str}'");
                    }
                    else
                    {
                        parsed_query_str = parsed_query_str.Replace($"@{i}", $"{query_param_arr[i]}");
                    }
                }
                return parsed_query_str;
            });

            return result_str_lst;
        }


        private static __QueryStr__ BuildQueryString(string logic_operator, FieldQuery[] field_queries, ref int param_index)
        {
            string query_str = "";
            var args = new List<object>();

            for (int i = 0; i < field_queries.Length; ++i, ++param_index)
            {
                if (i != 0) query_str += $" {logic_operator} ";
                //query_str += $"{query_lst.field_queries![i].field_name} {query_lst.field_queries![i].comparison_operator} @{param_index}";
                var operator_obj = QueryComparisonOperator.QueryComparisonOperatorDict[field_queries[i].comparison_operator!];
                query_str += operator_obj.function(field_queries[i].field_name!, $"@{param_index}"); // call operator formatting function
                args.Add(field_queries[i].compared_value!);
            }
            return new __QueryStr__ { query_str = query_str, query_params = args };
        }

        private static __QueryStr__ BuildQueryString(QueryList query_lst, ref int param_index) {
            string query_str = "";
            var args = new List<object>();
            
            // If OR then join by OR
            // If AND then join by AND
            if (!query_lst.field_queries.IsNullOrEmpty()) {
                __QueryStr__ field_query_str_obj = BuildQueryString(query_lst.logic_operator!, query_lst.field_queries!, ref param_index);
                query_str = field_query_str_obj.query_str!;
                args.AddRange(field_query_str_obj.query_params!);
                /*for (int i = 0; i < query_lst.field_queries!.Length; ++i, ++param_index)
                {
                    if (i != 0) query_str += $" {query_lst.logic_operator} ";
                    //query_str += $"{query_lst.field_queries![i].field_name} {query_lst.field_queries![i].comparison_operator} @{param_index}";
                    var operator_obj = QueryComparisonOperator.QueryComparisonOperatorDict[query_lst.field_queries![i].comparison_operator!];
                    query_str += operator_obj.function(query_lst.field_queries![i].field_name!, $"@{param_index}");
                    args.Add(query_lst.field_queries![i].compared_value!);
                }*/
            }

            // If subquery then recursively add BuildQueryString(subquery)
            if (!query_lst.sub_query_lists.IsNullOrEmpty()) {
                for (int i = 0; i < query_lst.sub_query_lists!.Length; ++i)
                {
                    __QueryStr__ sub_query_str_obj = BuildQueryString(query_lst.sub_query_lists![i], ref param_index);
                    if (String.IsNullOrEmpty(sub_query_str_obj.query_str)) continue;
                    if (!String.IsNullOrEmpty(query_str)) query_str += $" {query_lst.logic_operator} ";
                    query_str += $"({sub_query_str_obj.query_str})";
                    args.AddRange(sub_query_str_obj.query_params!);
                }
            }


            //object random_var = 1;
            //var changed_post_ID = Convert.ChangeType(postID, random_var);
            //string type_name = changed_post_ID.GetType().Name;
            return new __QueryStr__ { query_str = query_str, query_params = args };
        }

        private static List<__QueryStr__> BuildQueryStrings(QueryList query_lst) {
            int param_index = 0;
            var string_lst = new List<__QueryStr__>();
            if (query_lst.logic_operator == QueryLogicOperator.OR.ToString() || query_lst.sub_query_lists.IsNullOrEmpty()) {
                if (query_lst.field_queries.IsNullOrEmpty()) return string_lst;
                string_lst.Add(BuildQueryString(query_lst, ref param_index));
                return string_lst;
            }


            // separate into different strings to simplify query context
            if (!query_lst.field_queries.IsNullOrEmpty())
            {
                __QueryStr__ field_query_str_obj = BuildQueryString(query_lst.logic_operator!, query_lst.field_queries!, ref param_index);
                string_lst.Add(field_query_str_obj);
            }
            var parsed_sub_query_str_lst = query_lst.sub_query_lists!
                .Select(subquerylst => {
                    param_index = 0;
                    return BuildQueryString(subquerylst, ref param_index);
                })
                .Where(str_obj => !str_obj.query_str!.Trim().IsNullOrEmpty());
            string_lst.AddRange(parsed_sub_query_str_lst);
            return string_lst;
        }

        //public QueryLogicOperator logic_operator;
    }



}
