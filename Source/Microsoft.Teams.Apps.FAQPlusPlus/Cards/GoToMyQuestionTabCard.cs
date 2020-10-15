// <copyright file="GoToMyQuestionTabCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public class GoToMyQuestionTabCard
    {
        private readonly string manifestAppId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoToMyQuestionTabCard"/> class.
        /// </summary>
        /// <param name="configurationSettings">The configuration settings instance.</param>
        public GoToMyQuestionTabCard(ConfigurationSettings configurationSettings)
        {
            configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));
            this.manifestAppId = configurationSettings.ManifestAppId;
        }

        /// <summary>
        /// This method will construct the change language preference card for the user.
        /// </summary>
        /// <returns>User language change card.</returns>
        public Attachment GetCard()
        {
            AdaptiveCard userWelcomeCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = Strings.MyQuestionNotificationCardContent,
                        Wrap = true,
                        Weight = AdaptiveTextWeight.Bolder,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Title = Strings.MyQuestionLabelText,
                        Url = this.GetDeeplinkToMyQuestionTab(),
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = userWelcomeCard,
            };
        }

        /// <summary>
        /// Gets the deep link to event tab.
        /// If the passing in subEntityId is null, then it points to events tab.
        /// Otherwise, it points a spcific event, which triggers UI show the event in task module.
        /// </summary>
        /// <param name="subEntityId">The event id.</param>
        /// <returns>The deep link to the events tab.</returns>
        private Uri GetDeeplinkToMyQuestionTab(string subEntityId = null)
        {
            string context;
            if (!string.IsNullOrEmpty(subEntityId))
            {
                var contextObject = new
                {
                    subEntityId,
                };
                context = "context=" + Uri.EscapeDataString(JsonConvert.SerializeObject(contextObject));
            }
            else
            {
                context = string.Empty;
            }

            return new Uri(string.Format(
                CultureInfo.InvariantCulture,
                "https://teams.microsoft.com/l/entity/{0}/{1}?{2}",
                this.manifestAppId,
                "MyQuestions",
                context));
        }
    }
}
