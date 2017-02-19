namespace StoredProcedureProxy.Helpers
{
	public static class StringHelper
	{
		public static string ToParameterName(this string name)
		{
			return name == null || name.StartsWith("@")
				? name
				: "@" + name;
		}
	}
}
