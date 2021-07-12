using ClientDependency.Core;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Terrabec.PropertyEditors.EnumerateEmailTemplates
{
	[PropertyEditor(TerrabecEnumerateEmailTemplatesPropertyEditor.PropertyEditorAlias, "Terrabec Enumerate Email Templates", "/App_Plugins/Terrabec/PropertyEditors/EnumerateEmailTemplates/Views/editor.html?cache=1.0.0", ValueType = PropertyEditorValueTypes.Text, Group = "Email", Icon = "icon-users-alt")]
#if DEBUG
	[PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/Terrabec/PropertyEditors/EnumerateEmailTemplates/Scripts/enumerateemailtemplates.js?cache=1.0.0")]
#else
	[PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/Terrabec/PropertyEditors/EnumerateEmailTemplates/Scripts/enumerateemailtemplates.min.js?cache=1.0.0")]
#endif
	public class TerrabecEnumerateEmailTemplatesPropertyEditor : PropertyEditor
	{
		public const string PropertyEditorAlias = nameof(Terrabec) + nameof(EnumerateEmailTemplates);

		protected override PreValueEditor CreatePreValueEditor()
		{
			return new TerrabecEnumerateListsPreValueEditor();
		}

		public TerrabecEnumerateEmailTemplatesPropertyEditor()
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
			[PreValueField("definition", "Config", "/App_Plugins/Terrabec/PropertyEditors/EnumerateEmailTemplates/Views/config.html?cache=1.0.0", Description = "", HideLabel = true)]
			public PropertyDefinitionModel Definition { get; set; }

		}
	}
}
