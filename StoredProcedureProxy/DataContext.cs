using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Mappers;
using DataReaderMapper;
using StoredProcedureProxy.Helpers;

namespace StoredProcedureProxy
{
	public class DataContext : IDataContext
	{
		private readonly ConcurrentDictionary<Type, ProxyTypeCache> _cache = new ConcurrentDictionary<Type, ProxyTypeCache>();

		private readonly string _connectionString;
		private readonly IMapper _mapper;

		static DataContext()
		{
			MapperRegistry.Mappers.Insert(0, new DataReaderObjectMapper());
		}

		public DataContext(DataContextConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration), "Configuration must be not null");
			}
			configuration.Validate();

			_mapper = configuration.Mapper ?? new Mapper(new MapperConfiguration(config => { }));
			_connectionString = configuration.ConnectionString;
			Generator = configuration.Generator ?? new DefaultProxyGenerator();
			Activator = configuration.Activator ?? new DefaultProxyActivator(new DefaultMethodResolver());
			CommandBuilder = configuration.CommandBuilder ?? new DefaultCommandBuilder();

			// ReSharper disable once InvertIf
			if (configuration.PregenerateProxies)
			{
				configuration.BeforePregenerateProxies?.Invoke();
				PregenerateProxies();
			}
		}

		public IProxyGenerator Generator { get; }
		public IProxyActivator Activator { get; }
		public ICommandBuilder CommandBuilder { get; }
		public int Timeout { get; set; } = 60;

		public T GetProxy<T>() where T : IProxy
		{
			return (T)GetProxy(typeof(T));
		}

		public object GetProxy(Type interfaceType)
		{
			return CacheGetOrAdd(interfaceType).Instance;
		}

		public T GetProxy<T>(IExecutionContext executionContext) where T : IProxy
		{
			return (T)GetProxy(typeof(T), executionContext);
		}

		public object GetProxy(Type interfaceType, IExecutionContext executionContext)
		{
			return CacheGetOrAdd(interfaceType).CreateNewInstance(executionContext);
		}

		private ProxyTypeCache CacheGetOrAdd(Type interfaceType)
		{
			return _cache.GetOrAdd(interfaceType, t =>
			{
				var instanceType = Generator.Generate(interfaceType);
				return new ProxyTypeCache(interfaceType, instanceType, this);
			});
		}

		public virtual IExecutionContext CreateNewExecutionContext()
		{
			return new DefaultExecutionContext(_connectionString, Timeout);
		}

		public T Execute<T>(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			var disposeContext = false;
			if (executionContext == null)
			{
				executionContext = CreateNewExecutionContext();
				disposeContext = true;
			}

			try
			{
				var type = typeof(T);
				if (typeof(DataSet).IsAssignableFrom(type))
				{
					return (T)(object)ExecuteDataSet(typeof(T), descriptor, executionContext);
				}
				if (type.IsPrimitive() && descriptor.ReturnParameter == null)
				{
					return ExecuteScalar<T>(descriptor, executionContext);
				}
				if (descriptor.ReturnParameter != null)
				{
					return ExecuteNonQuery<T>(descriptor, executionContext);
				}

				var reader = ExecuteReader(descriptor, executionContext);
				if (typeof(IDataReader).IsAssignableFrom(type))
				{
					return (T)reader;
				}

				var result = _mapper.Map<T>(reader);
				reader.Close();

				return result;
			}
			finally
			{
				if (disposeContext)
				{
					executionContext.Dispose();
				}
			}
		}

		public T ExecuteScalar<T>(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			try
			{
				return (T)ExecuteScalar(descriptor, executionContext);
			}
			catch (Exception)
			{
				return default(T);
			}
		}

		public object ExecuteScalar(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			return CreateCommand(descriptor, executionContext, command =>
			{
				var result = command.ExecuteScalar();
				return descriptor.ReturnParameter != null
					? command.GetParameter(descriptor.ReturnParameter.Name).Value
					: result;
			});
		}

		public T ExecuteNonQuery<T>(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			if (descriptor.ReturnParameter == null)
			{
				throw new InvalidOperationException("Return parameter must have a value");
			}

			return CreateCommand(descriptor, executionContext, command =>
			{
				command.ExecuteNonQuery();
				return command.GetParameter(descriptor.ReturnParameter.Name).GetValue<T>();
			});
		}

		public int ExecuteNonQuery(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			return CreateCommand(descriptor, executionContext, command => command.ExecuteNonQuery());
		}

		public IDataReader ExecuteReader(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			return CreateCommand(descriptor, executionContext, command => command.ExecuteReader());
		}

		public DataSet ExecuteDataSet(StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			return ExecuteDataSet(typeof(DataSet), descriptor, executionContext);
		}

		public T ExecuteDataSet<T>(StoredProcedureDescriptor descriptor, IExecutionContext executionContext) where T: DataSet, new()
		{
			return (T)ExecuteDataSet(typeof(T), descriptor, executionContext);
		}

		private DataSet ExecuteDataSet(Type dataSetType, StoredProcedureDescriptor descriptor, IExecutionContext executionContext)
		{
			return CreateCommand(descriptor, executionContext, command =>
			{
				using (var dataAdapter = new SqlDataAdapter(command))
				{
					var dataSet = (DataSet)System.Activator.CreateInstance(dataSetType);
					dataAdapter.Fill(dataSet);
					return dataSet;
				}
			});
		}

		private T CreateCommand<T>(StoredProcedureDescriptor descriptor, IExecutionContext executionContext, Func<SqlCommand, T> fn)
		{
			var disposeContext = false;
			if (executionContext == null)
			{
				executionContext = CreateNewExecutionContext();
				disposeContext = true;
			}

			try
			{
				using (var command = CommandBuilder.Build(descriptor, executionContext))
				{
					return fn(command);
				}
			}
			finally
			{
				if (disposeContext)
				{
					executionContext.Dispose();
				}
			}
		}

		private void PregenerateProxies()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var proxyInterfaceType = typeof(IProxy);
			foreach (var assembly in assemblies)
			{
				try
				{
					var types = assembly.GetTypes().Where(t => t.IsInterface && t.GetInterfaces().Contains(proxyInterfaceType));
					foreach (var type in types)
					{
						CacheGetOrAdd(type);
					}
				}
				catch (ReflectionTypeLoadException)
				{
					Trace.TraceInformation($"Could not load types from the assembly: {assembly.FullName}");
				}
			}
		}
	}
}