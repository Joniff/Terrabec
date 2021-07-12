using System;
using System.Configuration;
using System.Web.Configuration;
using System.Xml;
using umbraco.cms.businesslogic.packager.standardPackageActions;
using umbraco.interfaces;
using Umbraco.Core.Logging;


namespace Terrabec.Config.PackageActions
{
	public class AddConfig : IPackageAction
	{
		public bool Execute(string packageName, XmlNode xmlData)
		{
			try
			{
				var config = WebConfigurationManager.OpenWebConfiguration("~");

				if (config.Sections[Models.Terrabec.Tag] == null)
				{
					var configSection = new Models.Terrabec();
					configSection.SectionInformation.ConfigSource = $"config\\{Models.Terrabec.Tag}.config";

					config.Sections.Add(Models.Terrabec.Tag, configSection);
					configSection.SectionInformation.ForceSave = true;
					config.Save(ConfigurationSaveMode.Full);
				}

				return true;
			}
			catch (Exception ex)
			{
				LogHelper.Error<AddConfig>("Error executing package action", ex);
			}

			return false;
		}

		public string Alias() => nameof(AddConfig);

		public bool Undo(string packageName, XmlNode xmlData)
		{
			try
			{
				var config = WebConfigurationManager.OpenWebConfiguration("~");

				if (config.Sections[Models.Terrabec.Tag] != null)
				{
					config.Sections.Remove(Models.Terrabec.Tag);
					config.Save(ConfigurationSaveMode.Full);
				}
				return true;
			}
			catch (Exception ex)
			{
				LogHelper.Error<AddConfig>("Error removing package action", ex);
			}

			return false;
		}

		public XmlNode SampleXml()
		{
			return helper.parseStringToXmlNode("<Action runat=\"install\" undo=\"true\" alias=\"AddConfig\" />");
		}

	}
}
