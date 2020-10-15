// <copyright file="TicketsProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Ticket provider helps in fetching and storing information in storage table.
    /// </summary>
    public class TicketsProvider : ITicketsProvider
    {
        private const string PartitionKey = "TicketInfo";
        private readonly Lazy<Task> initializeTask;
        private CloudTable ticketCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketsProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public TicketsProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="ticket">Represents ticket entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public Task UpsertTicketAsync(TicketEntity ticket)
        {
            ticket.PartitionKey = PartitionKey;
            ticket.RowKey = ticket.TicketId;

            if (ticket.Status > (int)TicketState.MaxValue)
            {
                throw new TicketValidationException($"The ticket status ({ticket.Status}) is not valid.");
            }

            return this.StoreOrUpdateTicketEntityAsync(ticket);
        }

        /// <summary>
        /// Delete ticket entity in table storage.
        /// </summary>
        /// <param name="ticket">ticketEntity.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is deleted.</returns>
        public Task DeleteTicketAsync(TicketEntity ticket)
        {
            ticket.PartitionKey = PartitionKey;
            ticket.RowKey = ticket.TicketId;
            ticket.IsDeleted = true;

            return this.StoreOrUpdateTicketEntityAsync(ticket);
        }

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="ticketId">ticket id received from bot based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<TicketEntity> GetTicketAsync(string ticketId)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no ticket created by end user and messaging extension is open by SME, table initialization is required before creating search index or datasource or indexer.
            if (string.IsNullOrEmpty(ticketId))
            {
                return null;
            }

            var searchOperation = TableOperation.Retrieve<TicketEntity>(PartitionKey, ticketId);
            var searchResult = await this.ticketCloudTable.ExecuteAsync(searchOperation).ConfigureAwait(false);

            return (TicketEntity)searchResult.Result;
        }

        /// <summary>
        /// Get list of tickets which were by user.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns>List of tickets.</returns>
        public async Task<IList<TicketEntity>> GetAllTicketAsync(string userName)
        {
            await this.EnsureInitializedAsync();

            string keyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);
            string userFilter = TableQuery.GenerateFilterCondition("RequesterUserPrincipalName", QueryComparisons.Equal, userName);
            string deletedFilter = TableQuery.GenerateFilterConditionForBool("IsDeleted", QueryComparisons.Equal, false);

            string combineKeyUserFilter = TableQuery.CombineFilters(keyFilter, TableOperators.And, userFilter);
            string combineFilter = TableQuery.CombineFilters(combineKeyUserFilter, TableOperators.And, deletedFilter);

            TableQuery<TicketEntity> query = new TableQuery<TicketEntity>()
                .Where(combineFilter);

            TableContinuationToken token = null;
            TableQuerySegment<TicketEntity> ticketResult;

            do
            {
                ticketResult = await this.ticketCloudTable.ExecuteQuerySegmentedAsync(query, token);
                token = ticketResult.ContinuationToken;
            }
            while (token != null);

            return ticketResult.Results;
        }

        /// <summary>
        /// Get list of tickets.
        /// </summary>
        /// <returns>List of tickets.</returns>
        public async Task<IList<TicketEntity>> GetTicketCountAsync()
        {
            await this.EnsureInitializedAsync();

            string keyFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKey);

            TableQuery<TicketEntity> query = new TableQuery<TicketEntity>()
                .Where(keyFilter);

            TableContinuationToken token = null;
            TableQuerySegment<TicketEntity> ticketResult;

            do
            {
                ticketResult = await this.ticketCloudTable.ExecuteQuerySegmentedAsync(query, token);
                token = ticketResult.ContinuationToken;
            }
            while (token != null);

            return ticketResult.Results;
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating table.
        /// </summary>
        /// <returns>Represent a task with initialized connection data.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Create tickets table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.ticketCloudTable = cloudTableClient.GetTableReference(Constants.TicketTableName);

            await this.ticketCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="entity">Represents ticket entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateTicketEntityAsync(TicketEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.ticketCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }
    }
}
