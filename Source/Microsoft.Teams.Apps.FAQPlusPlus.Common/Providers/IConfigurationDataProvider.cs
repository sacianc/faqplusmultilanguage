﻿// <copyright file="IConfigurationDataProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Interface of Configuration provider.
    /// </summary>
    public interface IConfigurationDataProvider
    {
        /// <summary>
        /// Save or update entity based on entity type.
        /// </summary>
        /// <param name="updatedData">Updated data received from view page.</param>
        /// <param name="entityType">EntityType received from view based on which appropriate row will replaced or inserted in table storage.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents updated data is saved or updated successfully while false indicates failure in saving or updating the updated data.</returns>
        Task<bool> UpsertEntityAsync(string updatedData, string entityType);

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="entityType">EntityType received from view based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/>Already saved entity detail.</returns>
        Task<string> GetSavedEntityDetailAsync(string entityType);

        /// <summary>
        /// Save or update LanguageKBConfigurationEntity based on entity type.
        /// </summary>
        /// <param name="languageCode">Language code from view based on which appropriate row will replaced or inserted in table storage.</param>
        /// <param name="languageKBConfiguration">Updated language specific knowledgebase configuration received from view page.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents updated data is saved or updated successfully while false indicates failure in saving or updating the updated data.</returns>
        Task<bool> UpsertLanguageKBConfigurationEntityAsync(string languageCode, LanguageKBConfigurationEntity languageKBConfiguration);

        /// <summary>
        /// Get already saved LanguageKBConfigurationEntity detail from storage table.
        /// </summary>
        /// <param name="languageCode">Language code from view based on which appropriate row will replaced or inserted in table storage.</param>
        /// <returns><see cref="Task"/>Already saved entity detail.</returns>
        Task<LanguageKBConfigurationEntity> GetSavedLanguageKBConfigurationEntityAsync(string languageCode);

        /// <summary>
        /// Get list of all LanguageKBConfigurationEntity from storage table.
        /// </summary>
        /// <returns><see cref="Task"/>List of LanguageKBConfigurationEntity detail.</returns>
        Task<List<LanguageKBConfigurationEntity>> GetAllLanguageKBConfigurationEntitiesAsync();
    }
}
