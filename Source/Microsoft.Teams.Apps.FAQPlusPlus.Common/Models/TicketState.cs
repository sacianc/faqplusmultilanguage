// <copyright file="TicketState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Represents the current status of a ticket.
    /// </summary>
    public enum TicketState
    {
        /// <summary>
        /// Represents an unanswered ticket.
        /// </summary>
        UnAnswered = 0,

        /// <summary>
        /// Represents a ticket that has been answered.
        /// </summary>
        Answered = 1,

        /// <summary>
        /// Sentinel value.
        /// </summary>
        MaxValue = Answered,
    }
}
