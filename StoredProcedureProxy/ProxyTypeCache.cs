using System;

namespace StoredProcedureProxy
{
	internal class ProxyTypeCache
	{
		private object _instance;
		private readonly IDataContext _dataContext;

		public ProxyTypeCache(Type interfaceType, Type instanceType, IDataContext dataContext)
		{
			InterfaceType = interfaceType;
			InstanceType = instanceType;
			_dataContext = dataContext;
		}

		public Type InterfaceType { get; set; }
		public Type InstanceType { get; set; }

		public object Instance
			=> _instance ?? (_instance = CreateNewInstance(null));

		public object CreateNewInstance(IExecutionContext executionContext)
		{
			return _dataContext.Activator.Activate(InstanceType, _dataContext, executionContext);
		}
	}
}