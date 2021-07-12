using Umbraco.Core;

namespace Terrabec.TreeController
{
	public class Register : ApplicationEventHandler 
	{
		public const string SectionName = nameof(Terrabec);
		public const string SectionAlias = "terrabec";
		private const string SectionIcon = "icon-message";
		private const int SectionSortOrder = 99;

		protected override void ApplicationStarted(UmbracoApplicationBase umbraco, ApplicationContext context) 
		{
			var section = context.Services.SectionService.GetByAlias(SectionAlias);
			if (section != null) 
			{
				return;
			}
			context.Services.SectionService.MakeNew(SectionName, SectionAlias, SectionIcon, SectionSortOrder);

			foreach (var group in context.Services.UserService.GetAllUserGroups())
			{
				group.AddAllowedSection(SectionAlias);
			}
			base.ApplicationStarted(umbraco, context);
		}
	}
}
