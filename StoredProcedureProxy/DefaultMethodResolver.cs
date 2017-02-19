using System.Linq;
using System.Reflection;
using StoredProcedureProxy.Helpers;

namespace StoredProcedureProxy
{
	internal class DefaultMethodResolver : IMethodResolver
	{
		public StoredProcedureDescriptor Resolve(MethodBase method, params ObjectValueReference[] arguments)
		{
			method =
				method.DeclaringType?.GetInterfaces()
					.Select(i => i.GetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray()))
					.FirstOrDefault() ?? method;

			var attribute = method.GetCustomAttribute<StoredProcedureAttribute>();
			var name = attribute?.Name ?? method.Name;
			var returnParameterName = attribute?.ReturnParameterName.ToParameterName();
			var parameters = method.GetParameters()
				.Select((p, i) =>
				{
					var parameterAttribute = p.GetCustomAttribute<ParameterAttribute>();
					var parameterName = (parameterAttribute?.Name ?? p.Name).ToParameterName();
					return new ParameterDescriptor(parameterName,
						i < arguments.Length ? arguments[i] : null,
						p.ParameterType,
						parameterAttribute?.SqlDbType,
						p.ParameterType.IsByRef,
						returnParameterName == parameterName
					);
				}).ToList();

			if (returnParameterName != null && parameters.All(p => p.Name != returnParameterName))
			{
				parameters.Add(new ParameterDescriptor(returnParameterName, new ObjectValueReference(),
					((MethodInfo)method).ReturnType, attribute.ReturnParameterSqlDbType, false, true));
			}

			return new StoredProcedureDescriptor(name, parameters.ToArray());
		}
	}
}