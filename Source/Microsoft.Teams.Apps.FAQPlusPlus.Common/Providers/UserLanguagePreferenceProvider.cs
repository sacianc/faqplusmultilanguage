// <copyright file="UserLanguagePreferenceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Represents user language preference provider.
    /// </summary>
    public class UserLanguagePreferenceProvider : IUserLanguagePreferenceProvider
    {
        private const string PartitionKey = "UserLanguagePreference";
        private readonly Lazy<Task> initializeTask;
        private CloudTable userLanguagePreferenceCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLanguagePreferenceProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public UserLanguagePreferenceProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <inheritdoc cref="UpsertLanguagePreferenceAsync(UserLanguagePreferenceEntity)"/>
        public Task UpsertLanguagePreferenceAsync(UserLanguagePreferenceEntity userLanguagePreference)
        {
            userLanguagePreference.PartitionKey = PartitionKey;
            userLanguagePreference.RowKey = userLanguagePreference.UserObjectId;
            return this.InsertOrUpdateUserLanguagePreferenceEntityAsync(userLanguagePreference);
        }

        /// <inheritdoc cref="GetLanguagePreferenceAsync(string)" />
        public async Task<UserLanguagePreferenceEntity> GetLanguagePreferenceAsync(string userObjectId)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(userObjectId))
            {
                return null;
            }

            var searchOperation = TableOperation.Retrieve<UserLanguagePreferenceEntity>(PartitionKey, userObjectId);
            var searchResult = await this.userLanguagePreferenceCloudTable.ExecuteAsync(searchOperation).ConfigureAwait(false);

            return (UserLanguagePreferenceEntity)searchResult.Result;
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
        /// Create UserLanguagePreference table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.userLanguagePreferenceCloudTable = cloudTableClient.GetTableReference(Constants.UserLanguagePreferenceTableName);

            await this.userLanguagePreferenceCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Inserts or updates UserLanguagePreference entity in table storage.
        /// </summary>
        /// <param name="entity">Represents UserLanguagePreference entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> InsertOrUpdateUserLanguagePreferenceEntityAsync(UserLanguagePreferenceEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.userLanguagePreferenceCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }
    }
}
