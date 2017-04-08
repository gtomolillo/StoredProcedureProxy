using System;
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

		public static object GetValue(this SqlParameter parameter, object defaultValue)
		{
			return parameter?.Value == null || parameter.Value == DBNull.Value ? defaultValue : parameter.Value;
		}

		public static T GetValue<T>(this SqlParameter parameter, T defaultValue)
		{
			return (T)parameter.GetValue((object)defaultValue);
		}

		public static T GetValue<T>(this SqlParameter parameter)
		{
			return parameter.GetValue(default(T));
		}
	}
}