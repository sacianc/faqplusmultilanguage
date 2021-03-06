﻿// <copyright file="SearchHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// Class that handles the search activities for messaging extension.
    /// </summary>
    public static class SearchHelper
    {
        /// <summary>
        /// Open requests command id in the manifest file.
        /// </summary>
        private const string OpenCommandId = "openrequests";

        /// <summary>
        ///  Recents command id in the manifest file.
        /// </summary>
        private const string RecentCommandId = "recents";

        /// <summary>
        /// Assigned requests command id in the manifest file.
        /// </summary>
        private const string AssignedCommandId = "assignedrequests";

        /// <summary>
        ///  Kb questions command id in the manifest file.
        /// </summary>
        private const string KnowledgebaseQuestionCommandId = "kbquestions";

        /// <summary>
        /// Get the results from Azure search service and populate the result (card + preview).
        /// </summary>
        /// <param name="languageCode">Current expert team's applicable language code.</param>
        /// <param name="query">Query which the user had typed in message extension search.</param>
        /// <param name="commandId">Command id to determine which tab in message extension has been invoked.</param>
        /// <param name="count">Count for pagination.</param>
        /// <param name="skip">Skip for pagination.</param>
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <param name="searchService">Search service.</param>
        /// <param name="knowledgeBaseSearchService">Knowledgebase search service.</param>
        /// <param name="activityStorageProvider">Activity storage provider.</param>
        /// <param name="appBaseUri">Application base uri.</param>
        /// <returns><see cref="Task"/> Returns MessagingExtensionResult which will be used for providing the card.</returns>
        public static async Task<MessagingExtensionResult> GetSearchResultAsync(
            string languageCode,
            string query,
            string commandId,
            int? count,
            int? skip,
            DateTimeOffset? localTimestamp,
            ISearchService searchService,
            IKnowledgeBaseSearchService knowledgeBaseSearchService,
            IActivityStorageProvider activityStorageProvider,
            string appBaseUri)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = AttachmentLayoutTypes.List,
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            IList<TicketEntity> searchServiceResults = new List<TicketEntity>();

            // commandId should be equal to Id mentioned in Manifest file under composeExtensions section.
            switch (commandId)
            {
                case RecentCommandId:
                    searchServiceResults = await searchService.SearchTicketsAsync(TicketSearchScope.RecentTickets, languageCode, query, count, skip).ConfigureAwait(false);
                    composeExtensionResult = GetMessagingExtensionResult(commandId, localTimestamp, searchServiceResults, appBaseUri);
                    break;

                case OpenCommandId:
                    searchServiceResults = await searchService.SearchTicketsAsync(TicketSearchScope.UnAnsweredTickets, languageCode, query, count, skip).ConfigureAwait(false);
                    composeExtensionResult = GetMessagingExtensionResult(commandId, localTimestamp, searchServiceResults, appBaseUri);
                    break;

                case AssignedCommandId:
                    searchServiceResults = await searchService.SearchTicketsAsync(TicketSearchScope.AnsweredTickets, languageCode, query, count, skip).ConfigureAwait(false);
                    composeExtensionResult = GetMessagingExtensionResult(commandId, localTimestamp, searchServiceResults, appBaseUri);
                    break;

                case KnowledgebaseQuestionCommandId:
                    var azureSearchEntities = await knowledgeBaseSearchService.GetAzureSearchEntitiesAsync(query).ConfigureAwait(false);

                    if (azureSearchEntities.Any())
                    {
                        var activitiesData = await activityStorageProvider.GetActivityEntitiesAsync().ConfigureAwait(false);
                        composeExtensionResult.Attachments = MessagingExtensionQnaCard.GetAllKbQuestionsCard(azureSearchEntities, activitiesData);
                    }

                    break;
            }

            return composeExtensionResult;
        }

        /// <summary>
        /// Get populated result to in messaging extension tab.
        /// </summary>
        /// <param name="commandId">Command id to determine which tab in message extension has been invoked.</param>
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <param name="searchServiceResults">List of tickets from Azure search service.</param>
        /// <param name="appBaseUri">Application base uri.</param>
        /// <returns><see cref="Task"/> Returns MessagingExtensionResult which will be shown in messaging extension tab.</returns>
        public static MessagingExtensionResult GetMessagingExtensionResult(
            string commandId,
            DateTimeOffset? localTimestamp,
            IList<TicketEntity> searchServiceResults,
            string appBaseUri)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = AttachmentLayoutTypes.List,
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            foreach (var ticket in searchServiceResults)
            {
                ThumbnailCard previewCard = new ThumbnailCard
                {
                    Title = ticket.Title,
                    Text = GetPreviewCardText(ticket, commandId, localTimestamp),
                };

                var selectedTicketAdaptiveCard = new MessagingExtensionTicketsCard(ticket);
                composeExtensionResult.Attachments.Add(selectedTicketAdaptiveCard.ToAttachment(localTimestamp, appBaseUri, false).ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return composeExtensionResult;
        }

        /// <summary>
        /// Get the text for the preview card for the result.
        /// </summary>
        /// <param name="ticket">Ticket object for ask an expert action.</param>
        /// <param name="commandId">Command id which indicate the action.</param>
        /// <param name="localTimestamp">Local time stamp.</param>
        /// <returns>Command id as string.</returns>
        private static string GetPreviewCardText(TicketEntity ticket, string commandId, DateTimeOffset? localTimestamp)
        {
            var ticketStatus = commandId != OpenCommandId ? $"<div style='white-space:nowrap'>{HttpUtility.HtmlEncode(Cards.CardHelper.GetTicketDisplayStatusForSme(ticket, null))}</div>" : string.Empty;
            var cardText = $@"<div>
                                <div style='white-space:nowrap'>
                                        #{HttpUtility.HtmlEncode(ticket.TicketId)} 
                                        | {HttpUtility.HtmlEncode(ticket.RequesterName)}
                                        | {HttpUtility.HtmlEncode(Cards.CardHelper.GetFormattedDateInUserTimeZone(ticket.DateCreated, localTimestamp))} 
                                        
                                </div> {ticketStatus}
                         </div>";

            return cardText.Trim();
        }
    }
}
