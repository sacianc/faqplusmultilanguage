// <copyright file="IUserLanguagePreferenceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of user language preference provider.
    /// </summary>
    public interface IUserLanguagePreferenceProvider
    {
        /// <summary>
        /// Insert or update user langauge preference entity to storage table.
        /// </summary>
        /// <param name="userLanguagePreference">User language preference entity received which will be replaced or inserted in table storage.</param>
        /// <returns><see cref="Task"/> that resolves successfully if the data was saved successfully.</returns>
        Task UpsertLanguagePreferenceAsync(UserLanguagePreferenceEntity userLanguagePreference);

        /// <summary>
        /// Get already saved user language preference entity detail from storage table.
        /// </summary>
        /// <param name="userObjectId">User object id based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<UserLanguagePreferenceEntity> GetLanguagePreferenceAsync(string userObjectId);
    }
}
