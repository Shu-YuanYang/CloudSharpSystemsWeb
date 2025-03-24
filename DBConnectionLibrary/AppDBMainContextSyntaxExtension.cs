using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLibrary
{

	public static class AppDBMainContextSyntaxExtension
	{

		private const string NOT_IMPLEMENTED_MESSAGE = "Syntax for this DB provider is not supported yet!";


		public static DbParameter SQLParameterType(this AppDBMainContext DBContext, string ParameterName, DbType DbType, object? Value=null, ParameterDirection Direction=ParameterDirection.Input) {
			switch (DBContext.Provider)
			{
				case AppDBMainContext.DBProvider.SQLSERVER:
					return new SqlParameter {
						ParameterName = ParameterName,
						DbType = DbType,
						Direction = Direction,
						Value = (Value == null)? DBNull.Value : Value
					};
				case AppDBMainContext.DBProvider.POSTGRESQL:
					return new NpgsqlParameter
					{
						ParameterName = ParameterName,
						DbType = DbType,
						Direction = Direction,
						Value = (Value == null) ? DBNull.Value : Value
					};
				default:
					throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
			}
		}


		private static string FormatSQLOutputParam(this AppDBMainContext DBContext, DbParameter Param) {
			switch (DBContext.Provider)
			{
				case AppDBMainContext.DBProvider.SQLSERVER:
					return $"@{Param.ParameterName} OUTPUT";
				case AppDBMainContext.DBProvider.POSTGRESQL:
					return "NULL";
				default:
					throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
			}
		}

		public static string FormatExecSPSQL(this AppDBMainContext DBContext, string ProcedureName, DbParameter[] Params) {
			string SQLCommand;
			var param_str_lst = Params.Select(p => 
				(p.Direction == ParameterDirection.Output)? DBContext.FormatSQLOutputParam(p) : $"@{p.ParameterName}"
			);
			string params_str = string.Join(", ", param_str_lst);

			switch (DBContext.Provider) {
				case AppDBMainContext.DBProvider.SQLSERVER:
					SQLCommand = $"EXEC {ProcedureName} {params_str}";
					break;
				case AppDBMainContext.DBProvider.POSTGRESQL:
					SQLCommand = $"CALL {ProcedureName}({params_str})";
					break;
				default:
					throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
			}
			return SQLCommand;
		}

		public static string FormatSelectTopSQL(this AppDBMainContext DBContext, string SQLCommand, int Rows) {
			SQLCommand = SQLCommand.TrimEnd(';'); // remove trailing ; if any
			SQLCommand = SQLCommand.Trim();
			string start_command = "SELECT";
			if (!SQLCommand.ToUpper().StartsWith(start_command))
				throw new FormatException("Top DB records selection must start with the SELECT command!");

			switch (DBContext.Provider)
			{
				case AppDBMainContext.DBProvider.SQLSERVER:
					SQLCommand = $"SELECT TOP {Rows} {SQLCommand.Substring(start_command.Length)}";
					break;
				case AppDBMainContext.DBProvider.POSTGRESQL:
					SQLCommand = $"{SQLCommand} LIMIT {Rows}";
					break;
				default:
					throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
			}

			return SQLCommand;
		}

		public static string FormatSelectCurrentTimestampSQL(this AppDBMainContext DBContext, string OutputParam) {
			string timestamp_str;
			switch (DBContext.Provider)
			{
				case AppDBMainContext.DBProvider.SQLSERVER:
					timestamp_str = $"SELECT {OutputParam} = GETDATE()";
					break;
				case AppDBMainContext.DBProvider.POSTGRESQL:
					timestamp_str = "SELECT CURRENT_TIMESTAMP";
					break;
				default:
					throw new NotImplementedException(NOT_IMPLEMENTED_MESSAGE);
			}
			return timestamp_str;
		}

	}
}
