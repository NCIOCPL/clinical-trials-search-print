using System;
using System.Collections.Generic;

namespace NCI.DataManager
{
    /// <summary>
    /// This class represents a single reuslt obtained after performing the search.
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// The record number for use in nvelocity template.
        /// </summary>
        public int RecNumber { get; set; }

        /// <summary>
        /// Long descritpion of the content item
        /// </summary>
        public string ShortTitle { get; set; }

        /// <summary>
        /// Long descritpion of the content item
        /// </summary>
        public string LongTitle { get; set; }

        /// <summary>
        /// Short Description of the content item.
        /// </summary>
        public string ShortDescription { get; set; }

        /// <summary>
        /// Long descritpion of the content item
        /// </summary>
        public string LongDescription { get; set; }

        /// <summary>
        /// The link to the complete content.
        /// </summary>
        public string HRef { get; set; }

        /// <summary>
        /// Updated date of the content
        /// </summary>
        public string UpdatedDate { get; set; }

        /// <summary>
        /// Question and Answer Url
        /// </summary>
        public string QandAUrl { get; set; }

        /// <summary>
        /// Imagee URL
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Video URL
        /// </summary>
        public string VideoUrl { get; set; }

        /// <summary>
        /// Audio URL
        /// </summary>
        public string AudioUrl { get; set; }

        /// <summary>
        /// Other language Url
        /// </summary>
        public string OtherlanguageUrl { get; set; }
        /// <summary>
        /// Language settings
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Posted date of the content.
        /// </summary>
        public string PostedDate { get; set; }

        /// <summary>
        /// Posted date of the content in long form (June 19, 2012)
        /// </summary>
        public string PostedDate_NewsPortalFormat { get; set; }

        /// <summary>
        /// Posted date of the content.
        /// </summary>
        public string DateDisplayMode { get; set; }

        public string ReviewedDate { get; set; }

        /// <summary>
        /// subheader
        /// </summary>
        public string SubHeader { get; set; }
        /// <summary>
        /// Source Path of dynamic list item image
        /// </summary>
        public string ImageSource { get; set; }
        /// <summary>
        /// Alt Text for dynamic list item image
        /// </summary>
        public string AltText { get; set; }

        /// <summary>
        /// the abbreviated source field from the external news link
        /// </summary>
        public string AbbreviatedSource { get; set; }

        /// <summary>
        /// alt text of an image in gloImageSl 
        /// </summary>
        public string Alt { get; set; }

        /// <summary>
        /// First two paragraphs of the blog body field
        /// </summary>
        public string BlogBody { get; set; }

        /// <summary>
        /// Url of Thumbnail image of a Global Image
        /// </summary>
        public string ThumbnailURL { get; set; }

        /// <summary>
        /// Author of Content
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Content Type
        /// </summary>
        public string ContentType { get; set; }


        public string ContentID { get; set; }

        /// <summary>
        /// Used with external news links.  Does external resource require a subscription?
        /// </summary>
        public Boolean SubscriptionRequired { get; set; }

        /// <summary>
        /// Whether or not Disqus comments have been enabled
        /// </summary>
        public Boolean AllowComments { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        public int FileSize { get; set; }

        /// <summary>
        /// File's media type
        /// </summary>
        public String MimeType { get; set; }

        /// <summary>
        /// Most recent date to show on lists
        /// </summary>
        public String DateForLists { get; set; }

        /// <summary>
        /// Most recent date to show on lists in Spanish
        /// </summary>
        public String DateForListsEs { get; set; }

        /// <summary>
        /// Formatted "Posted" date to show on Blog Series page
        /// </summary>
        public String DateForBlogs { get; set; }

        /// <summary>
        /// Formatted "Posted" date to show on Spanish Blog Series page
        /// </summary>
        public String DateForBlogsEs { get; set; }

        public SearchResult()
        {
            ShortDescription = string.Empty;
            LongTitle = string.Empty;
            LongDescription = string.Empty;
            UpdatedDate = string.Empty;
            PostedDate = string.Empty;
            HRef = string.Empty;
            ThumbnailURL = string.Empty;
            Alt = string.Empty;
            BlogBody = string.Empty;
            Author = string.Empty;
            MimeType = string.Empty;
            FileSize = 0;
            DateForLists = string.Empty;
            DateForListsEs = string.Empty;
            DateForBlogs = string.Empty;
            DateForBlogsEs = string.Empty;
        }
    }
}
