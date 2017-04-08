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
				var sqlParameter = command.Parameters.AddWithValue(parameter.Name, parameter.Value.Coalesce(DBNull.Value));
				if (parameter.IsOut || parameter.IsReturn)
				{
					sqlParameter.Direction = ParameterDirection.InputOutput;
					sqlParameter.SqlDbType = parameter.SqlDbType;
					sqlParameter.Size = parameter.Size;
				}
			}

			return command;
		}
	}
}