// <copyright file="ChangeLanguageCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using System.Linq;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public static class ChangeLanguageCard
    {
        /// <summary>
        /// This method will construct the change language preference card for the user.
        /// </summary>
        /// <param name="botSupportedLanguages">List of bot supported language detail objects.</param>
        /// <returns>User language change card.</returns>
        public static Attachment GetCard(List<BotLanguageDetail> botSupportedLanguages)
        {
            var cardActions = botSupportedLanguages.Select(item => new AdaptiveSubmitAction
                {
                    Title = item.Name,
                    Data = new AdaptiveSubmitActionData
                    {
                        LanguageSelected = item.Code,
                    },
                });
            AdaptiveCard userWelcomeCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Strings.ChangeLanguageHeaderText,
                        Wrap = true,
                        Size = AdaptiveTextSize.Large,
                    },
                },
                Actions = new List<AdaptiveAction>(cardActions),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = userWelcomeCard,
            };
        }
    }
}