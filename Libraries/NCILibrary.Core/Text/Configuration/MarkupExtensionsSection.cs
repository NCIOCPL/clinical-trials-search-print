using System;
using System.Configuration;

namespace NCI.Text.Configuration
{
    public class MarkupExtensionsSection : ConfigurationSection
    {
        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)base["enabled"]; }
        }

        [ConfigurationProperty("returnHandlerErrorsAsOutput", IsRequired = false, DefaultValue = false)]
        public bool ReturnHandlerErrorsAsOutput
        {
            get { return (bool)base["returnHandlerErrorsAsOutput"]; }
        }

        [ConfigurationProperty("loaders")]
        public LoaderElementCollection MarkupExtensionLoaders
        {
            get { return (LoaderElementCollection)base["loaders"]; }
        }
    }
}
