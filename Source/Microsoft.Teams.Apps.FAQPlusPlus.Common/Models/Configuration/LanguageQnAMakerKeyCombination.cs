// <copyright file="LanguageQnAMakerKeyCombination.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration
{
    /// <summary>
    /// Represents the language code qna maker key details object.
    /// </summary>
    public class LanguageQnAMakerKeyCombination
    {
        /// <summary>
        /// Gets or sets language code.
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets language name.
        /// </summary>
        public string LanguageName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the language is default.
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// Gets or sets qna maker subscription key.
        /// </summary>
        public string QnAMakerSubscriptionKey { get; set; }

        /// <summary>
        /// Gets or sets qna maker host url.
        /// </summary>
        public string QnAMakerHostUrl { get; set; }
    }
}
