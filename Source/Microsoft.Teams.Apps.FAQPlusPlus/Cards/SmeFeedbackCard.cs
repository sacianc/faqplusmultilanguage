// <copyright file="SmeFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a feedback through bot menu or from response card.
    /// </summary>
    public static class SmeFeedbackCard
    {
        /// <summary>
        /// This method will construct the card for SME team which will have the
        /// feedback details given by the user.
        /// </summary>
        /// <param name="data">User activity payload.</param>
        /// <param name="userDetails">User details.</param>
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <returns>Sme facing feedback notification card.</returns>
        public static Attachment GetCard(ShareFeedbackCardPayload data, TeamsChannelAccount userDetails, DateTimeOffset? localTimestamp)
        {
            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
               {
                   new AdaptiveTextBlock()
                   {
                       Text = Strings.SMEFeedbackHeaderText,
                       Weight = AdaptiveTextWeight.Bolder,
                       Size = AdaptiveTextSize.Large,
                   },
               },
                Actions = new List<AdaptiveAction>
               {
                   new AdaptiveOpenUrlAction
                   {
                       Title = string.Format(CultureInfo.InvariantCulture, Strings.ChatTextButton, userDetails?.GivenName),
                       UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userDetails.UserPrincipalName)}",
                   },
               },
            };

            smeFeedbackCard.Body.Add(new AdaptiveFactSet
            {
                Facts = BuildFactSet(data, localTimestamp, userDetails?.Name),
            });

            // Question asked fact and view article show card is available when feedback is on QnA Maker response.
            if (!string.IsNullOrWhiteSpace(data.KnowledgeBaseAnswer) && !string.IsNullOrWhiteSpace(data.UserQuestion))
            {
                smeFeedbackCard.Actions.AddRange(new List<AdaptiveAction>
                {
                    new AdaptiveShowCardAction
                    {
                        Title = Strings.ViewArticleButtonText,
                        Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                        {
                            Body = new List<AdaptiveElement>
                            {
                               new AdaptiveTextBlock
                               {
                                   Text = CardHelper.TruncateStringIfLonger(data.KnowledgeBaseAnswer, CardHelper.KnowledgeBaseAnswerMaxDisplayLength),
                                   Wrap = true,
                               },
                            },
                        },
                    },
                });
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = smeFeedbackCard,
            };
        }

        // Return the display string for the given rating
        private static string GetRatingDisplayText(string rating)
        {
            if (!Enum.TryParse(rating, out FeedbackRating value))
            {
                throw new ArgumentException($"{rating} is not a valid rating value", nameof(rating));
            }

            return Strings.ResourceManager.GetString($"{rating}RatingText", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Return the appropriate fact set based on the state and information in the ticket.
        /// </summary>
        /// <param name="data">User activity payload.</param>
        /// <param name="localTimestamp">The current timestamp.</param>
        /// <param name="userName">The user who sent the feeback.</param>
        /// <returns>The fact set showing the necessary details.</returns>
        private static List<AdaptiveFact> BuildFactSet(ShareFeedbackCardPayload data, DateTimeOffset? localTimestamp, string userName)
        {
            List<AdaptiveFact> factList = new List<AdaptiveFact>();
            if (string.IsNullOrEmpty(data.TicketId))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.QuestionMessageFeedbackText,
                    Value = data.UserQuestion,
                });
            }
            else
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.QuestionMessageFeedbackText,
                    Value = string.Format(CultureInfo.InvariantCulture, Strings.SMEFeedbackTicketIDFact, data.TicketId, data.UserQuestion),
                });
            }

            factList.Add(new AdaptiveFact
            {
                Title = Strings.RatingTitle,
                Value = GetRatingDisplayText(data?.Rating),
            });

            // Description fact is available in the card only when user enters description text.
            if (!string.IsNullOrWhiteSpace(data.Description))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.DescriptionText,
                    Value = CardHelper.TruncateStringIfLonger(data.Description, CardHelper.DescriptionMaxDisplayLength),
                });
            }

            factList.Add(new AdaptiveFact
            {
                Title = Strings.DateFactTitle,
                Value = CardHelper.GetFormattedDateInUserTimeZone(DateTime.Now, localTimestamp),
            });

            factList.Add(new AdaptiveFact
            {
                Title = Strings.ProvidedByFact,
                Value = userName,
            });

            return factList;
        }
    }
}