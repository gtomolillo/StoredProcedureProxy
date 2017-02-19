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
				if (parameter.Value == null)
				{
					command.Parameters.Add(parameter.Name, parameter.SqlDbType);
				}
				else
				{
					command.Parameters.AddWithValue(parameter.Name, parameter.Value.Coalesce(DBNull.Value));
				}
			}

			return command;
		}
	}
}