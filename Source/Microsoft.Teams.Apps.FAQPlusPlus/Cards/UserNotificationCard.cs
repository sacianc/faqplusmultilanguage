// <copyright file="UserNotificationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// Creates a user notification card from a ticket.
    /// </summary>
    public class UserNotificationCard
    {
        private readonly TicketEntity ticket;
        private readonly string manifestAppId;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket to create a card from.</param>
        /// <param name="configurationSettings">The configuration settings instance.</param>
        public UserNotificationCard(TicketEntity ticket, ConfigurationSettings configurationSettings)
        {
            configurationSettings = configurationSettings ?? throw new ArgumentNullException(nameof(configurationSettings));

            this.ticket = ticket;
            this.manifestAppId = configurationSettings.ManifestAppId;
        }

        /// <summary>
        /// Returns a user notification card for the ticket.
        /// </summary>
        /// <param name="message">The status message to add to the card.</param>
        /// <param name="activityLocalTimestamp">Local time stamp of user activity.</param>
        /// <returns>An adaptive card as an attachment.</returns>
        public Attachment ToAttachment(string message, DateTimeOffset? activityLocalTimestamp)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            if (message == Strings.NotificationCardContent)
            {
                card.Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = message,
                        Wrap = true,
                        Size = AdaptiveTextSize.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.NotificationCardDetailContent,
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                      Facts = this.BuildFactSet(this.ticket, activityLocalTimestamp),
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.MyQuestionNotificationCardContent,
                        Wrap = true,
                    },
                };

                card.Actions = this.BuildActions();
            }
            else
            {
                card.Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = string.Format(CultureInfo.InvariantCulture, message, this.ticket.TicketId),
                        Wrap = true,
                        Size = AdaptiveTextSize.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = this.ticket.Title,
                        Wrap = true,
                        Size = AdaptiveTextSize.Small,
                    },
                    new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(UpdateTicketResponsePayload.Answer),
                        Placeholder = Strings.ResponsePlaceholderText,
                        IsMultiline = true,
                        Value = string.Format(CultureInfo.InvariantCulture, Strings.SMEReplyCardAnswerByExpert, this.ticket.AnswerBySME),
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.SMEReplyCardFooterMessage,
                        Wrap = true,
                    },
                };

                card.Actions = this.BuildReplyActions();
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Create an adaptive card action that starts a chat with the user.
        /// </summary>
        /// <returns>Adaptive card action for starting chat with user.</returns>
        protected AdaptiveAction CreateChatWithExpertAction()
        {
            var messageToSend = string.Format(CultureInfo.InvariantCulture, Strings.SMEUserChatMessage, this.ticket.Title);
            var encodedMessage = Uri.EscapeDataString(messageToSend);

            return new AdaptiveOpenUrlAction
            {
                Title = Strings.ChatWithExpertButton,
                Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(this.ticket.AssignedToUserPrincipalName)}&message={encodedMessage}"),
            };
        }

        /// <summary>
        /// Having the necessary adaptive actions built.
        /// </summary>
        /// <returns>A list of adaptive card actions.</returns>
        private List<AdaptiveAction> BuildActions()
        {
            UserNotificationCard userNotificationCard = this;
            return new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction()
                    {
                        Title = Strings.MyQuestionLabelText,
                        Url = userNotificationCard.GetDeeplinkToMyQuestionTab(),
                    },
                };
        }

        /// <summary>
        /// Having the necessary adaptive actions built.
        /// </summary>
        /// <returns>A list of adaptive card actions.</returns>
        private List<AdaptiveAction> BuildReplyActions()
        {
            UserNotificationCard userNotificationCard = this;
            List<AdaptiveAction> actions = new List<AdaptiveAction>();

            actions.Add(this.CreateChatWithExpertAction());
            actions.Add(new AdaptiveSubmitAction
            {
                Title = Strings.ShareFeedbackButtonText,
                Data = new ResponseCardPayload
                {
                    MsTeams = new CardAction
                    {
                        Type = ActionTypes.MessageBack,
                        DisplayText = Strings.ShareFeedbackDisplayText,
                        Text = Constants.ShareFeedback,
                    },
                    UserQuestion = this.ticket.Title,
                    KnowledgeBaseAnswer = this.ticket.KnowledgeBaseAnswer,
                    KnowledgeBaseQuestion = this.ticket.KnowledgeBaseQuestion,
                    TicketId = this.ticket.TicketId,
                },
            });

            return actions;
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

        /// <summary>
        /// Building the fact set to render out the user facing details.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <param name="activityLocalTimestamp">The local timestamp.</param>
        /// <returns>The adaptive facts.</returns>
        private List<AdaptiveFact> BuildFactSet(TicketEntity ticket, DateTimeOffset? activityLocalTimestamp)
        {
            List<AdaptiveFact> factList = new List<AdaptiveFact>();
            factList.Add(new AdaptiveFact
            {
                Title = Strings.TicketIDFact,
                Value = CardHelper.TruncateStringIfLonger(this.ticket.TicketId, CardHelper.TicketIdMaxDisplayLength),
            });

            factList.Add(new AdaptiveFact
            {
                Title = Strings.TitleFact,
                Value = CardHelper.TruncateStringIfLonger(this.ticket.Title, CardHelper.TitleMaxDisplayLength),
            });

            factList.Add(new AdaptiveFact
            {
                Title = Strings.StatusFactTitle,
                Value = CardHelper.GetUserTicketDisplayStatus(this.ticket),
            });

            // if (!string.IsNullOrEmpty(ticket.Description))
            // {
            //    factList.Add(new AdaptiveFact
            //    {
            //        Title = Strings.DescriptionFact,
            //        Value = CardHelper.TruncateStringIfLonger(this.ticket.Description, CardHelper.DescriptionMaxDisplayLength),
            //    });
            // }
            factList.Add(new AdaptiveFact
            {
                Title = Strings.DateCreatedDisplayFactTitle,
                Value = CardHelper.GetFormattedDateInUserTimeZone(this.ticket.DateCreated, activityLocalTimestamp),
            });

            return factList;
        }
    }
}