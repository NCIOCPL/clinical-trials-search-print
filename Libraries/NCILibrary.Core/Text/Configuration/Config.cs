using System;
using System.Configuration;

namespace NCI.Text.Configuration
{
    public static class Config
    {
        private static readonly string markupExtensionsPath = "nci/text/markupExtensions";
        private static MarkupExtensionsSection _markupExtensions;

        public static MarkupExtensionsSection MarkupExtensions
        {
            get
            {
                if (_markupExtensions == null)
                {
                    _markupExtensions = (MarkupExtensionsSection)ConfigurationManager.GetSection(markupExtensionsPath);

                    // When the markupExtensions sectionGroup isn't defined, _markupExtensions comes back null.
                    if (_markupExtensions == null)
                    {
                        throw new ConfigurationErrorsException(String.Format("Could not load markup extensions configuration from {0}.  Please ensure the markup extensions section group and configuration element have been defined in the configuration file.", markupExtensionsPath));
                    }
                }

                return _markupExtensions;
            }
        }
    }
}
