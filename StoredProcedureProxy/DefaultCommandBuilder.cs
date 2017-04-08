using System;
using System.Data;
using System.Data.SqlClient;
using StoredProcedureProxy.Helpers;

namespace StoredProcedureProxy
{
	internal class DefaultCommandBuilder : ICommandBuilder
	{
		public SqlCommand Build(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			var command = new SqlCommand(descriptor.Name, executionContext.Connection)
			{
				CommandTimeout = executionContext.Timeout,
				CommandType = CommandType.StoredProcedure
			};

			if (executionContext.Transaction != null)
			{
				command.Transaction = executionContext.Transaction;
			}

			foreach (var parameter in descriptor.Parameters)
			{
				var sqlParameter = parameter.SqlDbType == SqlDbType.Structured
					? command.Parameters.Add(parameter.Name, parameter.SqlDbType)
					: command.Parameters.AddWithValue(parameter.Name, parameter.Value.Coalesce(DBNull.Value));

				// ReSharper disable once InvertIf
				if (parameter.IsOut || parameter.IsReturn)
				{
					sqlParameter.Direction = ParameterDirection.InputOutput;
					sqlParameter.SqlDbType = parameter.SqlDbType;
					sqlParameter.Size = parameter.Size;
				}
				// ReSharper disable once InvertIf
				if (parameter.SqlDbType == SqlDbType.Structured)
				{
					sqlParameter.TypeName = parameter.SqlTypeName;
					sqlParameter.Value = ((IStructuredType)parameter.Value)?.ToDataTable();
				}
			}

			return command;
		}
	}
}