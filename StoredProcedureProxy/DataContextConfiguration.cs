using System;
using AutoMapper;

namespace StoredProcedureProxy
{
	public class DataContextConfiguration
	{
		public string ConnectionString { get; set; }
		public int Timeout { get; set; } = 60;
		public bool PregenerateProxies { get; set; } = true;
		public Action BeforePregenerateProxies { get; set; }
		public IProxyGenerator Generator { get; set; }
		public IProxyActivator Activator { get; set; }
		public ICommandBuilder CommandBuilder { get; set; }
		public IMapper Mapper { get; set; }

		internal void Validate()
		{
			if (string.IsNullOrEmpty(ConnectionString))
			{
				throw new ArgumentException("Connection String must have a value");
			}
		}
	}
}
