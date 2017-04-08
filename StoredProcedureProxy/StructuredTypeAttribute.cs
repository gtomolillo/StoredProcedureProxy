using System;

namespace StoredProcedureProxy
{
	[AttributeUsage(AttributeTargets.Class)]
	public class StructuredTypeAttribute : Attribute
	{
		public StructuredTypeAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}