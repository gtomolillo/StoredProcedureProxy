namespace StoredProcedureProxy.Helpers
{
	public static class ObjectHelper
	{
		public static T Coalesce<T>(this T instance, T valueIfNull)
		{
			return Equals(instance, default(T))
				? valueIfNull
				: instance;
		}
	}
}
