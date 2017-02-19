using System;

namespace StoredProcedureProxy
{
	public interface IProxyActivator
	{
		object Activate(Type type, IDataContext dataContext, IExecutionContext executionContext);
		T Activate<T>(IDataContext dataContext, IExecutionContext executionContext);

		IMethodResolver Resolver { get; }
	}
}