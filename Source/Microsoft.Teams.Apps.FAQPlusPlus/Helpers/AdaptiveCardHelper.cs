// <copyright file="AdaptiveCardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Adaptive card helper class for tickets.
    /// </summary>
    public static class AdaptiveCardHelper
    {
        /// <summary>
        /// Helps to get the expert submit card.
        /// </summary>
        /// <param name="currentLanguageCode">Current applicable language code.</param>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<TicketEntity> AskAnExpertSubmitText(
            string currentLanguageCode,
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken,
            ITicketsProvider ticketsProvider)
        {
            var askAnExpertSubmitTextPayload = ((JObject)message.Value).ToObject<AskAnExpertCardPayload>();

            // Validate required fields.
            if (string.IsNullOrWhiteSpace(askAnExpertSubmitTextPayload?.Title))
            {
                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { AskAnExpertCard.GetCard(askAnExpertSubmitTextPayload) },
                };
                await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                return null;
            }

            var userDetails = await GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return await CreateTicketAsync(currentLanguageCode, message, askAnExpertSubmitTextPayload, userDetails, ticketsProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Helps to get the expert submit card.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<Attachment> ShareFeedbackSubmitText(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var shareFeedbackSubmitTextPayload = ((JObject)message.Value).ToObject<ShareFeedbackCardPayload>();

            // Validate required fields.
            if (!Enum.TryParse(shareFeedbackSubmitTextPayload?.Rating, out FeedbackRating rating))
            {
                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { ShareFeedbackCard.GetCard(shareFeedbackSubmitTextPayload) },
                };
                await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                return null;
            }

            var teamsUserDetails = await GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return SmeFeedbackCard.GetCard(shareFeedbackSubmitTextPayload, teamsUserDetails, message?.LocalTimestamp);
        }

        /// <summary>
        /// Get the account details of the expert in a 1:1 chat with the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="activityId">ActivityId of the conversation.</param>
        /// <param name="userName">The expert name</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<TeamsChannelAccount> GetUserDetailsInChannelChatAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken,
          string activityId,
          string userName)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetActivityMembersAsync(turnContext, activityId, cancellationToken).ConfigureAwait(false);
            foreach (ChannelAccount member in members)
            {
                if (member.Name == userName)
                {
                    return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(member));
                }
            }

            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }

        /// <summary>
        /// Get the account details of the user in a 1:1 chat with the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task<TeamsChannelAccount> GetUserDetailsInPersonalChatAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }

        /// <summary>
        /// Create a new ticket from the input.
        /// </summary>
        /// <param name="currentLanguageCode">Current applicable language code.</param>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="data">Represents the submit data associated with the Ask An Expert card.</param>
        /// <param name="member">Teams channel account detailing user Azure Active Directory details.</param>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        /// <returns>TicketEntity object.</returns>
        private static async Task<TicketEntity> CreateTicketAsync(
            string currentLanguageCode,
            IMessageActivity message,
            AskAnExpertCardPayload data,
            TeamsChannelAccount member,
            ITicketsProvider ticketsProvider)
        {
            IList<TicketEntity> ticketList = await ticketsProvider.GetTicketCountAsync();
            TicketEntity ticketEntity = new TicketEntity
            {
                LanguageCode = currentLanguageCode,
                TicketId = (10000 + ticketList.Count).ToString(), // Guid.NewGuid().ToString(),
                Status = (int)TicketState.UnAnswered,
                DateCreated = DateTime.UtcNow,
                Title = data.Title,
                Description = data.Description,
                RequesterName = member.Name,
                RequesterUserPrincipalName = member.UserPrincipalName,
                RequesterGivenName = member.GivenName,
                RequesterConversationId = message.Conversation.Id,
                LastModifiedByName = message.From.Name,
                LastModifiedByObjectId = message.From.AadObjectId,
                UserQuestion = data.UserQuestion,
                KnowledgeBaseAnswer = data.KnowledgeBaseAnswer,
                KnowledgeBaseQuestion = data.KnowledgeBaseQuestion,
            };

            await ticketsProvider.UpsertTicketAsync(ticketEntity).ConfigureAwait(false);

            return ticketEntity;
        }
    }
}
