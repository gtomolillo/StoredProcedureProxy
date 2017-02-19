using System;
using System.Linq;

namespace StoredProcedureProxy
{
	public class StoredProcedureDescriptor
	{
		public StoredProcedureDescriptor(string name, ParameterDescriptor[] parameters = null)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name), "Name must be not null");
			}

			Name = name;
			Parameters = parameters ?? new ParameterDescriptor[0];

			ReturnParameter = Parameters.FirstOrDefault(p => p.IsReturn);
		}

		public string Name { get; }
		public ParameterDescriptor[] Parameters { get; }
		public ParameterDescriptor ReturnParameter { get; }
	}
}