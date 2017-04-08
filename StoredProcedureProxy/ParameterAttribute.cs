using System;
using System.Data;

namespace StoredProcedureProxy
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class ParameterAttribute : Attribute
	{
		public ParameterAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }

		public SqlDbType SqlDbType
		{
			get { return ActualSqlDbType ?? SqlDbType.Variant; }
			set { ActualSqlDbType = value; }
		}

		public SqlDbType? ActualSqlDbType { get; private set; }
	}
}