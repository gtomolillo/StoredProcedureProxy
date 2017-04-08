using System;

namespace StoredProcedureProxy
{
	public interface IDataContext
	{
		IProxyGenerator Generator { get; }
		IProxyActivator Activator { get; }

		T GetProxy<T>() where T : IProxy;
		object GetProxy(Type interfaceType);
		T GetProxy<T>(IExecutionContext executionContext) where T : IProxy;
		object GetProxy(Type interfaceType, IExecutionContext executionContext);

		IExecutionContext CreateNewExecutionContext();

		void ExecuteVoid(StoredProcedureDescriptor descriptor, IExecutionContext executionContext);
		T Execute<T>(StoredProcedureDescriptor descriptor, IExecutionContext executionContext);
	}
}