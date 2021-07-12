using System;
using LinqToDB.Mapping;
using Newtonsoft.Json;
using Terrabec.Connectors;

namespace Terrabec.Queues.Sql.Persistance
{
	[Table("Terrabec_EmailTracking")]  
	public class EmailTracking
	{  
		[PrimaryKey, Identity] 
		public int Id { get; set; }  

		private DateTime submitted;

		[Column(Storage = nameof(submitted)), NotNull]
		public DateTime Submitted
		{
			get => DateTime.SpecifyKind(submitted, DateTimeKind.Utc);
			set => submitted = value.ToUniversalTime();
		}

		private DateTime? completed;

		[Column(Storage = nameof(completed)), Nullable]
		public DateTime? Completed
		{
			get => completed == null ? (DateTime?) null : DateTime.SpecifyKind((DateTime) completed, DateTimeKind.Utc);
			set => completed = value == null ? (DateTime?) null : ((DateTime) value).ToUniversalTime();
		}

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 255), NotNull]
		public string ConnectionId { get; set; }  

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 255), NotNull]
		public string Feature { get; set; }  

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 255), NotNull]
		public string EmailTemplateId { get; set; }  

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 1024), Nullable]
		public string FromName { get; set; }  

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 1024), Nullable]
		public string FromEmail { get; set; }  

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 1024), Nullable]
		public string ToName { get; set; }  

		[Column(DataType = LinqToDB.DataType.NVarChar, Length = 1024), Nullable]
		public string ToEmail { get; set; }  

		[Column(Name = nameof(Fields), DataType = LinqToDB.DataType.NVarChar, Length = int.MaxValue), Nullable]
		public string fields { get; private set; }

		[ColumnAlias(nameof(Fields))]
		public IContactFields Fields
		{
			get => JsonConvert.DeserializeObject<ContactFields>(fields);
			set => fields = JsonConvert.SerializeObject(value);
		}

		[Column(Name = nameof(Attachments), DataType = LinqToDB.DataType.NVarChar, Length = int.MaxValue), Nullable]
		public string attachments { get; private set; }  

		[ColumnAlias(nameof(Attachments))]
		public IAttachments Attachments
		{
			get => JsonConvert.DeserializeObject<Attachments>(attachments);
			set => attachments = JsonConvert.SerializeObject(value);
		}

	}  
}
