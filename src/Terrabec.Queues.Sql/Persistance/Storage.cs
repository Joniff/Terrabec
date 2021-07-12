using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using LinqToDB;

namespace Terrabec.Queues.Sql.Persistance
{
	public partial class Storage : LinqToDB.Data.DataConnection
	{
		private static int InitCount = 0;

		private SqlConfig Config;

		public class EmailTrackingOperation
		{
			Storage Db;
			SqlConfig Config;

			public IQueryable<EmailTracking> Query() => Db.GetTable<EmailTracking>().TableName(Config.DatabaseTable).AsQueryable<EmailTracking>();
			public int Insert(EmailTracking emailTrack) => emailTrack.Id = Convert.ToInt32((decimal) Db.InsertWithIdentity<EmailTracking>(emailTrack, Config.DatabaseTable));
			public int Update(EmailTracking emailTrack, params string[] ColumnNames) => 
				ColumnNames.Length == 0 ? Db.Update<EmailTracking>(emailTrack, Config.DatabaseTable) :
				Db.Update<EmailTracking>(emailTrack, (record,column) => ColumnNames.Any(c => c == column.MemberName), Config.DatabaseTable);

			internal EmailTrackingOperation(Storage db, SqlConfig config)
			{
				Db = db;
				Config = config;
			}
		}

		public EmailTrackingOperation EmailTracking;

		//public Storage(SqlConfig config) : base(ConfigurationManager.ConnectionStrings[config.DataSourceName].ConnectionString)
		public Storage(SqlConfig config) : base(config.DataSourceName)
		{
			Config = config;
			EmailTracking = new EmailTrackingOperation(this, config);
			if (Interlocked.CompareExchange(ref InitCount, 1, 0) == 0)
			{
				var sp = this.DataProvider.GetSchemaProvider().GetSchema(this);
				if (!sp.Tables.Any(t => t.TableName == Config.DatabaseTable))
				{
					this.CreateTable<EmailTracking>(Config.DatabaseTable);
				}
			}
		} 
	}
}
