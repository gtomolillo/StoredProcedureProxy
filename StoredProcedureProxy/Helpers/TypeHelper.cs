using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace StoredProcedureProxy.Helpers
{
	internal static class TypeHelper
	{
		private static readonly Type NullableType = typeof(Nullable<>);
		private static readonly Type[] List;

		private static readonly Dictionary<Type, SqlDbType> SqlDbTypeMapping = new Dictionary<Type, SqlDbType>
		{
			{ typeof(long), SqlDbType.BigInt },
			{ typeof(ulong), SqlDbType.BigInt },
			{ typeof(int), SqlDbType.Int },
			{ typeof(uint), SqlDbType.Int },
			{ typeof(short), SqlDbType.SmallInt },
			{ typeof(ushort), SqlDbType.SmallInt },
			{ typeof(float), SqlDbType.Float },
			{ typeof(double), SqlDbType.Float },
			{ typeof(decimal), SqlDbType.Decimal },
			{ typeof(sbyte), SqlDbType.Binary },
			{ typeof(byte), SqlDbType.Binary },
			{ typeof(byte[]), SqlDbType.VarBinary },
			{ typeof(bool), SqlDbType.Bit },
			{ typeof(char), SqlDbType.NChar },
			{ typeof(char[]), SqlDbType.NVarChar },
			{ typeof(string), SqlDbType.NVarChar },
			{ typeof(DateTime), SqlDbType.DateTime },
			{ typeof(DateTimeOffset), SqlDbType.DateTimeOffset },
			{ typeof(Guid), SqlDbType.UniqueIdentifier },
			{ typeof(TimeSpan), SqlDbType.Time },
		};

		static TypeHelper()
		{
			var types = new[]
			{
				typeof (char), typeof (Guid),
				typeof (bool), typeof (byte), typeof (short), typeof (int),
				typeof (long), typeof (float), typeof (double), typeof (decimal),
				typeof (sbyte), typeof (ushort), typeof (uint), typeof (ulong),
				typeof (DateTime), typeof (DateTimeOffset), typeof (TimeSpan),
			};

			List = types
				.Concat(types.Select(t => NullableType.MakeGenericType(t)))
				.Concat(new [] { typeof(Enum), typeof(string) })
				.ToArray();
		}

		public static bool IsPrimitive(this Type type)
		{
			if (type == null)
			{
				return false;
			}

			if (List.Any(x => x.IsAssignableFrom(type)))
				return true;

			var nut = Nullable.GetUnderlyingType(type);
			return nut != null && nut.IsEnum;
		}

		public static SqlDbType ToSqlDbType(this Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type), "Type must be not null");
			}
			
			if (!type.IsPrimitive())
			{
				return SqlDbType.Structured;
			}
			if (type.IsEnum)
			{
				type = Enum.GetUnderlyingType(type);
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition() == NullableType)
			{
				type = Nullable.GetUnderlyingType(type);
			}
			if (SqlDbTypeMapping.ContainsKey(type))
			{
				return SqlDbTypeMapping[type];
			}

			throw new ArgumentOutOfRangeException(nameof(type), $"No existing mapping for the type: {type.FullName}");
		}

		public static Type GetUnderlyingType(this Type type)
		{
			if (type == null)
			{
				return null;
			}

			return type.IsByRef
				? type.GetElementType()
				: type;
		}
	}
}