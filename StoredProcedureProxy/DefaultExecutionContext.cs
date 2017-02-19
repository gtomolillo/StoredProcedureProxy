using System;
using System.Data;
using System.Data.SqlClient;

namespace StoredProcedureProxy
{
	internal class DefaultExecutionContext : IExecutionContext
	{
		public SqlConnection Connection { get; }
		public SqlTransaction Transaction { get; private set; }
		public int Timeout { get; }

		public DefaultExecutionContext(string connectionString, int timeout)
		{
			Connection = new SqlConnection(connectionString);
			Connection.Open();
			Timeout = timeout;
		}

		public void Dispose()
		{
			Connection.Dispose();
		}

		private void CheckTransaction()
		{
			if (Transaction == null)
			{
				throw new InvalidOperationException("Impossible perform this operation. No existing transaction");
			}
		}

		private void CheckExistingTransaction()
		{
			if (Transaction != null)
			{
				throw new InvalidOperationException("Impossible perform this operation. A transaction already exists");
			}
		}

		public void BeginTransaction()
		{
			CheckExistingTransaction();
			Transaction = Connection.BeginTransaction();
		}

		public void BeginTransaction(string transactionName)
		{
			CheckExistingTransaction();
			Transaction = Connection.BeginTransaction(transactionName);
		}

		public void BeginTransaction(IsolationLevel isolationLevel)
		{
			CheckExistingTransaction();
			Transaction = Connection.BeginTransaction(isolationLevel);
		}

		public void BeginTransaction(IsolationLevel isolationLevel, string transactionName)
		{
			CheckExistingTransaction();
			Transaction = Connection.BeginTransaction(isolationLevel, transactionName);
		}

		public void Rollback()
		{
			CheckTransaction();
			Transaction.Rollback();
		}

		public void Rollback(string transactionName)
		{
			CheckTransaction();
			Transaction.Rollback(transactionName);
		}

		public void Commit()
		{
			CheckTransaction();
			Transaction.Commit();
		}

		public void Save(string savePointName)
		{
			CheckTransaction();
			Transaction.Save(savePointName);
		}
	}
}