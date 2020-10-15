// <copyright file="LanguageQnAMakerSubscriptionKeySettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents Language qna maker subscription key settings object.
    /// </summary>
    public class LanguageQnAMakerSubscriptionKeySettings
    {
        /// <summary>
        /// Gets or sets list of language qna maker key combinations.
        /// </summary>
        public List<LanguageQnAMakerKeyCombination> LanguageQnAMakerKeyCombinations { get; set; }
    }
}
