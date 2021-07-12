using ClientDependency.Core;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Terrabec.PropertyEditors.EnumerateLists
{
	[PropertyEditor(TerrabecEnumerateListsPropertyEditor.PropertyEditorAlias, "Terrabec Enumerate Lists", "/App_Plugins/Terrabec/PropertyEditors/EnumerateLists/Views/editor.html?cache=1.0.0", ValueType = PropertyEditorValueTypes.Text, Group = "Email", Icon = "icon-layers-alt")]
#if DEBUG
	[PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/Terrabec/PropertyEditors/EnumerateLists/Scripts/enumeratelists.js?cache=1.0.0")]
#else
	[PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/Terrabec/PropertyEditors/EnumerateLists/Scripts/enumeratelists.min.js?cache=1.0.0")]
#endif
	public class TerrabecEnumerateListsPropertyEditor : PropertyEditor
	{
		public const string PropertyEditorAlias = nameof(Terrabec) + nameof(EnumerateLists);

		protected override PreValueEditor CreatePreValueEditor()
		{
			return new TerrabecEnumerateListsPreValueEditor();
		}

		public TerrabecEnumerateListsPropertyEditor()
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

		internal class TerrabecEnumerateListsPreValueEditor : PreValueEditor
		{
			[PreValueField("definition", "Config", "/App_Plugins/Terrabec/PropertyEditors/EnumerateLists/Views/config.html?cache=1.0.0", Description = "", HideLabel = true)]
			public PropertyDefinitionModel Definition { get; set; }

		}
	}
}
