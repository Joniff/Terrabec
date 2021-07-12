using System.Collections.Generic;

namespace Terrabec.Connectors
{
	public class ContactFields : List<ContactField>, IContactFields
	{
		IEnumerator<IContactField> IEnumerable<IContactField>.GetEnumerator() => this.GetEnumerator();
	}
}
