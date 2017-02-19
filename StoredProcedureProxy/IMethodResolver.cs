using System.Reflection;

namespace StoredProcedureProxy
{
	public interface IMethodResolver
	{
		StoredProcedureDescriptor Resolve(MethodBase method, params ObjectValueReference[] arguments);
	}
}