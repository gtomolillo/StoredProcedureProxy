using System;
using System.Data;
using System.Data.SqlClient;

namespace StoredProcedureProxy
{
	public interface IExecutionContext : IDisposable
	{
		SqlConnection Connection { get; }
		SqlTransaction Transaction { get; }
		int Timeout { get; }

		void BeginTransaction();
		void BeginTransaction(string transactionName);
		void BeginTransaction(IsolationLevel isolationLevel);
		void BeginTransaction(IsolationLevel isolationLevel, string transactionName);
		void Rollback();
		void Rollback(string transactionName);
		void Commit();
		void Save(string savePointName);
	}
}