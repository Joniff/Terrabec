using System;
using Terrabec.Modules;

namespace Terrabec.Queues.EnumerateEmailsSent
{
	public interface IEnumerateEmailsSentFeature : IFeature
	{
		IEnumerateEmailsSentResults ExecuteEnumerateEmailsSentFeature(bool? sent, TimeSpan? submittedInLast);
	}
}
