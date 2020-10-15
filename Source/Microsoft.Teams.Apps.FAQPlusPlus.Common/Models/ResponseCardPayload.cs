// <copyright file="ResponseCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Represents the payload of a response card.
    /// </summary>
    public class ResponseCardPayload : TeamsAdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the question that was asked originally asked by the user.
        /// </summary>
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the response given by the bot to the user.
        /// </summary>
        public string KnowledgeBaseAnswer { get; set; }

        /// <summary>
        /// Gets or sets the qnaId given the question.
        /// </summary>
        public int QnAId { get; set; }

        /// <summary>
        /// Gets or sets the previous question id.
        /// </summary>
        public int PreviousQuestionId { get; set; }

        /// <summary>
        /// Gets or sets the previous question given the question.
        /// </summary>
        public string PreviousUserQuery { get; set; }

        /// <summary>
        /// Gets or sets the response given by the bot to the user.
        /// </summary>
        public string KnowledgeBaseQuestion { get; set; }

        /// <summary>
        /// Gets or sets the ticketid to be sent to the SME team along with the feedback
        /// provided by the user on response given by bot calling QnA Maker service.
        /// </summary>
        public string TicketId { get; set; }

    }
}
