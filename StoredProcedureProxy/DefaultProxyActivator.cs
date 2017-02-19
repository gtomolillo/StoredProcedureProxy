using System;

namespace StoredProcedureProxy
{
	internal class DefaultProxyActivator : IProxyActivator
	{
		public DefaultProxyActivator(IMethodResolver resolver)
		{
			if (resolver == null)
				throw new ArgumentNullException(nameof(resolver), "Resolver must be not null");

			Resolver = resolver;
		}

		public object Activate(Type type, IDataContext dataContext, IExecutionContext executionContext)
		{
			return Activator.CreateInstance(type, Resolver, dataContext, executionContext);
		}

		public T Activate<T>(IDataContext dataContext, IExecutionContext executionContext)
		{
			return (T)Activate(typeof(T), dataContext, executionContext);
		}

		public IMethodResolver Resolver { get; }
	}
}