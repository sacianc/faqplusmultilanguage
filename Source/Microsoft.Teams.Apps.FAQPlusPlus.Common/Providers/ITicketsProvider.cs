// <copyright file="ITicketsProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of Tickets provider.
    /// </summary>
    public interface ITicketsProvider
    {
        /// <summary>
        /// Save or update ticket entity.
        /// </summary>
        /// <param name="ticket">Ticket received from bot based on which appropriate row will replaced or inserted in table storage.</param>
        /// <returns><see cref="Task"/> that resolves successfully if the data was saved successfully.</returns>
        Task UpsertTicketAsync(TicketEntity ticket);

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="ticketId">ticket id received from bot based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<TicketEntity> GetTicketAsync(string ticketId);

        /// <summary>
        /// Get list of tickets which were raised by particular user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>List of tickets.</returns>
        Task<IList<TicketEntity>> GetAllTicketAsync(string userName);

        /// <summary>
        /// Delete ticket entity.
        /// </summary>
        /// <param name="ticket">Ticket received from tab will be deleted in table storage.</param>
        /// <returns><see cref="Task"/> that resolves successfully if the data was deleted successfully.</returns>
        Task DeleteTicketAsync(TicketEntity ticket);

        /// <summary>
        /// Get all tickets from storage table.
        /// </summary>
        /// <returns><see cref="Task"/> List of tickets.</returns>
        Task<IList<TicketEntity>> GetTicketCountAsync();
    }
}
