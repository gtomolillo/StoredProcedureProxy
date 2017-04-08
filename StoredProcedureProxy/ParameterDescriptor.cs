using System;
using System.Data;
using StoredProcedureProxy.Helpers;

namespace StoredProcedureProxy
{
	public class ParameterDescriptor
	{
		public ParameterDescriptor(string name, ObjectValueReference value, Type type, SqlDbType? sqlDbType, int size, bool isOut, bool isReturn)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name), "Name must be not null");
			}
			if (value?.Value == null && type == null)
			{
				throw new ArgumentNullException(nameof(value), "Value or Type must be not null");
			}
			if (isOut && value == null)
			{
				throw new ArgumentNullException(nameof(value), "Value must be not null where the parameter is defined as out");
			}

			Name = name;
			Reference = value ?? new ObjectValueReference();
			IsOut = isOut;
			IsReturn = isReturn;
			Type = (value?.Value?.GetType() ?? type).GetUnderlyingType();
			SqlDbType = sqlDbType ?? type.ToSqlDbType();
			Size = size;
		}

		public string Name { get; }
		public ObjectValueReference Reference { get; }
		public object Value
		{
			get { return Reference?.Value; }
			set { Reference.Value = value; }
		}
		public bool IsOut { get; set; }
		public bool IsReturn { get; set; }
		public Type Type { get; }
		public SqlDbType SqlDbType { get; }
		public int Size { get; }
	}
}