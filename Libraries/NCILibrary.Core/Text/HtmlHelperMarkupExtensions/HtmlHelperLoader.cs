using System;
using NCI.Text;

namespace NCI.Web
{
    /// <summary>
    /// Provides access to a keyed collection of delegates that create generic markup extension 
    /// handlers.
    /// </summary>
    public class HtmlHelperLoader : MarkupExtensionLoader
    {
        protected override void LoadHandlers()
        {
            Add(() => new DateHandler());
            Add(() => new FlashHandler());
        }
    }
}