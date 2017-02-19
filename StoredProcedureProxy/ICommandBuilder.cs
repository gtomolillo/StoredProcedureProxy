using System.Data.SqlClient;

namespace StoredProcedureProxy
{
	public interface ICommandBuilder
	{
		SqlCommand Build(StoredProcedureDescriptor descriptor, IExecutionContext executionContext);
	}
}