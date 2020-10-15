namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Language and QNA Details from the web.config.
    /// </summary>
    public class LanguageQnaDetails1
    {
        /// <summary>
        /// Gets or sets the LanguageCode.
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language is default.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Gets or sets the QnAMakerSubscriptionKey.
        /// </summary>
        public string QnAMakerSubscriptionKey { get; set; }
    }
}
