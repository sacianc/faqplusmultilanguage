// <copyright file="LanguageSelectionCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process language selection card, user selects language preference.
    /// </summary>
    public static class LanguageSelectionCard
    {
        /// <summary>
        /// This method will construct the user language selection card.
        /// </summary>
        /// <param name="changeLanguageMessageText">Gets selected language.</param>
        /// <returns>User language selection card.</returns>
        public static Attachment GetCard(string changeLanguageMessageText)
        {
            AdaptiveCard languageCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            languageCard.Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                    Text = changeLanguageMessageText,
                    Wrap = true,
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = languageCard,
            };
        }
    }
}
