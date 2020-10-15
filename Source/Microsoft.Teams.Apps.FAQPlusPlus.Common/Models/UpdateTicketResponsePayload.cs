// <copyright file="UpdateTicketResponsePayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data payload of Action.Submit to update or give answer to ticket.
    /// </summary>
    public class UpdateTicketResponsePayload
    {
        /// <summary>
        /// Action to submit answer.
        /// </summary>
        public const string RespondAction = "Respond";

        /// <summary>
        /// Action to submit answer.
        /// </summary>
        public const string AddRespondAction = "AddRespond";

        /// <summary>
        /// Action to update answer in KB.
        /// </summary>
        public const string UpdateResponseAction = "UpdateResponse";

        /// <summary>
        /// Gets or sets the ticket id.
        /// </summary>
        [JsonProperty("ticketId")]
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets the action to perform on the ticket.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the answer to be given by SME.
        /// </summary>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the answer to be given by SME.
        /// </summary>
        [JsonProperty("answerForRespond")]
        public string AnswerForRespond { get; set; }

        /// <summary>
        /// Gets or sets the add or append action to perform on the ticket.
        /// </summary>
        [JsonProperty("addorAppendAction")]
        public string AddorAppendAction { get; set; }
    }
}
