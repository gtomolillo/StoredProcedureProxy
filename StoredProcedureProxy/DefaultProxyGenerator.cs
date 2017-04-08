using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StoredProcedureProxy
{
	public class DefaultProxyGenerator : IProxyGenerator
	{
		private static readonly Type DataContextType = typeof(IDataContext);
		private static readonly Type ExecutionContextType = typeof(IExecutionContext);
		private static readonly Type MethodResolverType = typeof(IMethodResolver);
		private static readonly Type MethodBaseType = typeof(MethodBase);
		private static readonly Type ObjectType = typeof(object);
		private static readonly Type ObjectValueReferenceType = typeof(ObjectValueReference);
		private static readonly Type ObjectValueReferenceArrayType = typeof(ObjectValueReference[]);
		private static readonly Type StoredProcedureDescriptorType = typeof(StoredProcedureDescriptor);

		private static readonly MethodInfo ObjectValueReferenceValueGetMethod =
			ObjectValueReferenceType.GetProperty("Value").GetMethod;

		private static readonly MethodInfo GetCurrentMethodMethodInfo = MethodBaseType.GetMethod("GetCurrentMethod",
			BindingFlags.Public | BindingFlags.Static);

		private static readonly MethodInfo ResolveMethodInfo = MethodResolverType.GetMethod("Resolve",
			BindingFlags.Public | BindingFlags.Instance);

		private static readonly MethodInfo ExecuteMethodInfo = DataContextType.GetMethod("Execute",
			BindingFlags.Public | BindingFlags.Instance);

		private readonly ModuleBuilder _moduleBuilder;

		public DefaultProxyGenerator()
		{
			var assemblyName = new AssemblyName($"ProxyGenerator-{Guid.NewGuid()}");
			var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			_moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
		}

		public Type Generate<T>()
		{
			return Generate(typeof(T));
		}

		public Type Generate(Type interfaceType)
		{
			var typeBuilder = GenerateClass(interfaceType);

			var resolverField = GenerateField(typeBuilder, "_resolver", MethodResolverType);
			var dataContextField = GenerateField(typeBuilder, "_dataContext", DataContextType);
			var executionContextField = GenerateField(typeBuilder, "_executionContext", ExecutionContextType);

			GenerateConstructor(typeBuilder, new[] { MethodResolverType, DataContextType, ExecutionContextType }, il =>
			{
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Stfld, resolverField);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_2);
				il.Emit(OpCodes.Stfld, dataContextField);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_3);
				il.Emit(OpCodes.Stfld, executionContextField);
			});

			foreach (var method in interfaceType.GetMethods())
			{
				var parameters = method.GetParameters();
				GenerateMethod(typeBuilder, method, il =>
				{
					var methodBase = il.DeclareLocal(MethodBaseType);
					var descriptor = il.DeclareLocal(StoredProcedureDescriptorType);
					var arguments = il.DeclareLocal(ObjectValueReferenceArrayType);
					var objectReference = il.DeclareLocal(ObjectValueReferenceType);
					var result = il.DeclareLocal(method.ReturnType);

					il.Emit(OpCodes.Ldc_I4, parameters.Length);
					il.Emit(OpCodes.Newarr, ObjectValueReferenceType);
					il.Emit(OpCodes.Stloc, arguments);
					for (var i = 0; i < parameters.Length; i++)
					{
						var ctor = ObjectValueReferenceType.GetConstructor(new[] { typeof(object) });
						if (ctor == null)
						{
							throw new Exception();
						}
						if (parameters[i].IsOut)
						{
							il.Emit(OpCodes.Ldnull);
						}
						else
						{
							il.Emit(OpCodes.Ldarg_S, i + 1);
							if (parameters[i].ParameterType.IsValueType)
							{
								il.Emit(OpCodes.Box, parameters[i].ParameterType);
							}
						}
						il.Emit(OpCodes.Newobj, ctor);
						il.Emit(OpCodes.Stloc, objectReference);

						il.Emit(OpCodes.Ldloc, arguments);
						il.Emit(OpCodes.Ldc_I4, i);
						il.Emit(OpCodes.Ldloc, objectReference);
						il.Emit(OpCodes.Stelem_Ref);
						//if (parameters[i].ParameterType.IsValueType)
						//{
						//}
						//else
						//{
						//	il.Emit(OpCodes.Stelem, parameters[i].ParameterType);
						//}
					}

					il.Emit(OpCodes.Nop);
					il.Emit(OpCodes.Call, GetCurrentMethodMethodInfo);
					il.Emit(OpCodes.Stloc, methodBase);

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, resolverField);
					il.Emit(OpCodes.Ldloc, methodBase);
					il.Emit(OpCodes.Ldloc, arguments);
					il.Emit(OpCodes.Call, ResolveMethodInfo);
					il.Emit(OpCodes.Stloc, descriptor);

					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, dataContextField);
					il.Emit(OpCodes.Ldloc, descriptor);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, executionContextField);
					il.Emit(OpCodes.Call, ExecuteMethodInfo.MakeGenericMethod(method.ReturnType));
					il.Emit(OpCodes.Stloc, result);

					for (var i = 0; i < parameters.Length; i++)
					{
						if (parameters[i].ParameterType.IsByRef)
						{
							il.Emit(OpCodes.Ldloc, arguments);
							il.Emit(OpCodes.Ldc_I4, i);
							il.Emit(OpCodes.Ldelem, ObjectValueReferenceType);
							il.Emit(OpCodes.Call, ObjectValueReferenceValueGetMethod);
							il.Emit(OpCodes.Starg_S, i + 1);
						}
					}

					il.Emit(OpCodes.Ldloc, result);
				});
			}

			return typeBuilder.CreateType();
		}

		protected virtual TypeBuilder GenerateClass(Type interfaceType, string name = null)
		{
			return _moduleBuilder.DefineType(name ?? $"Proxy-{interfaceType.Name}",
				TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass |
				TypeAttributes.AutoLayout, null, new[] { interfaceType });
		}

		protected virtual void GenerateConstructor(TypeBuilder typeBuilder, Type[] arguments, Action<ILGenerator> body)
		{
			var ctor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, arguments);

			var baseCtor = ObjectType.GetConstructor(new Type[0]);

			var il = ctor.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			if (baseCtor != null) il.Emit(OpCodes.Call, baseCtor);

			body(il);

			il.Emit(OpCodes.Ret);
		}

		protected virtual FieldBuilder GenerateField(TypeBuilder typeBuilder, string name, Type type, FieldAttributes attributes = FieldAttributes.Private)
		{
			return typeBuilder.DefineField(name, type, attributes);
		}

		protected virtual void GenerateMethod(TypeBuilder typeBuilder, MethodInfo method, Action<ILGenerator> body)
		{
			GenerateMethod(typeBuilder, method.Name,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, method.ReturnType,
				method.GetParameters(), body);
		}

		protected virtual void GenerateMethod(TypeBuilder typeBuilder, string name, MethodAttributes attributes,
			Type returnType, ParameterInfo[] parameters, Action<ILGenerator> body)
		{
			var methodBuilder = typeBuilder.DefineMethod(name, attributes, returnType,
				parameters.Select(p => p.ParameterType).ToArray());

			for (var i = 0; i < parameters.Length; i++)
			{
				var parameterAttributes = ParameterAttributes.None;
				if (parameters[i].IsIn)
				{
					parameterAttributes |= ParameterAttributes.In;
				}
				if (parameters[i].IsOut)
				{
					parameterAttributes |= ParameterAttributes.Out;
				}
				if (parameters[i].HasDefaultValue)
				{
					parameterAttributes |= ParameterAttributes.HasDefault;
				}
				if (parameters[i].IsLcid)
				{
					parameterAttributes |= ParameterAttributes.Lcid;
				}
				if (parameters[i].IsLcid)
				{
					parameterAttributes |= ParameterAttributes.Optional;
				}

				var parameterBuilder = methodBuilder.DefineParameter(i + 1, parameterAttributes, parameters[i].Name);

				if (parameters[i].HasDefaultValue)
				{
					parameterBuilder.SetConstant(parameters[i].RawDefaultValue);
				}
			}			

			var il = methodBuilder.GetILGenerator();
			body(il);
			il.Emit(OpCodes.Ret);
		}
	}
}