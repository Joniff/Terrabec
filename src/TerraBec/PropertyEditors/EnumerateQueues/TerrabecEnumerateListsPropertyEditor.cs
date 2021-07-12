using ClientDependency.Core;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Terrabec.PropertyEditors.EnumerateQueues
{
	[PropertyEditor(TerrabecEnumerateQueuesPropertyEditor.PropertyEditorAlias, "Terrabec Enumerate Queues", "/App_Plugins/Terrabec/PropertyEditors/EnumerateQueues/Views/editor.html?cache=1.0.0", ValueType = PropertyEditorValueTypes.Text, Group = "Email", Icon = "icon-layers-alt")]
#if DEBUG
	[PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/Terrabec/PropertyEditors/EnumerateQueues/Scripts/enumeratequeues.js?cache=1.0.0")]
#else
	[PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/Terrabec/PropertyEditors/EnumerateQueues/Scripts/enumeratequeues.min.js?cache=1.0.0")]
#endif
	public class TerrabecEnumerateQueuesPropertyEditor : PropertyEditor
	{
		public const string PropertyEditorAlias = nameof(Terrabec) + nameof(EnumerateQueues);

		protected override PreValueEditor CreatePreValueEditor()
		{
			return new TerrabecEnumerateQueuesPreValueEditor();
		}

		public TerrabecEnumerateQueuesPropertyEditor()
		{
			_defaultPreVals = new Dictionary<string, object>
			{
				{ "definition", "{}" }
			};
		}

		private IDictionary<string, object> _defaultPreVals;
		public override IDictionary<string, object> DefaultPreValues
		{
			get { return _defaultPreVals; }
			set { _defaultPreVals = value; }
		}

		internal class TerrabecEnumerateQueuesPreValueEditor : PreValueEditor
		{
			[PreValueField("definition", "Config", "/App_Plugins/Terrabec/PropertyEditors/EnumerateQueues/Views/config.html?cache=1.0.0", Description = "", HideLabel = true)]
			public PropertyDefinitionModel Definition { get; set; }

		}
	}
}
