using System.Data;

namespace StoredProcedureProxy
{
	public interface IStructuredType
	{
		DataTable ToDataTable();
	}
}