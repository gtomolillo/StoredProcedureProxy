using System.Data.SqlClient;

namespace StoredProcedureProxy.Helpers
{
	public static class SqlCommandHelper
	{
		public static SqlParameter GetParameter(this SqlCommand command, string parameterName)
		{
			var index = command?.Parameters.IndexOf(parameterName);
			return index >= 0
				? command.Parameters[index.Value]
				: null;
		}
	}
}
