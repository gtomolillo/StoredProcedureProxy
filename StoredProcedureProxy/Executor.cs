using System.Data;
using System.Data.SqlClient;

namespace StoredProcedureProxy
{
	public class Executor
	{
		private readonly string _connectionString;

		public Executor(string connectionString)
		{
			_connectionString = connectionString;
		}

		public int ExecuteNonQuery()
		{
			using (var connection = GetConnection())
			{
				using (var command = new SqlCommand())
				{
					command.CommandText = "";
					command.CommandType = CommandType.StoredProcedure;
					command.Connection = connection;
					
					return command.ExecuteNonQuery();
				}
			}
		}

		private SqlConnection GetConnection()
		{
			return new SqlConnection(_connectionString);
		}
	}
}