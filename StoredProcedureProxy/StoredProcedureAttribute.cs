using System;
using System.Data;

namespace StoredProcedureProxy
{
	[AttributeUsage(AttributeTargets.Method)]
	public class StoredProcedureAttribute : Attribute
	{
		public StoredProcedureAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }

		public string ReturnParameterName { get; set; }
		public SqlDbType? ReturnParameterSqlDbType { get; set; }
		public int ReturnParameterSize { get; set; }
	}
}