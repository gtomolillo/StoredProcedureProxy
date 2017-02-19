namespace StoredProcedureProxy
{
	public class ObjectValueReference
	{
		public ObjectValueReference()
		{
			
		}

		public ObjectValueReference(object value)
		{
			Value = value;
		}

		public object Value { get; set; }
	}
}