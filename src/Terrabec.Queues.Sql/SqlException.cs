using System;

namespace Terrabec.Queues.Sql
{
	internal class SqlException : Exception
	{
		public int StatusCode { get; set; }

		public SqlException(int statusCode, string message) : base(message)
		{
			StatusCode = statusCode;
		}
	}
}
