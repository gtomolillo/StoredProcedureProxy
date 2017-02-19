using System;

namespace StoredProcedureProxy
{
	public interface IProxyGenerator
	{
		Type Generate<T>();
		Type Generate(Type interfaceType);
	}
}