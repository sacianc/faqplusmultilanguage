﻿// <copyright file="ISearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of Search Service provider.
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Provide search result for table to be used by SME based on Azure search service.
        /// </summary>
        /// <param name="searchScope">Scope of the search.</param>
        /// <param name="languageCode">Language code applicable to the expert team where the search is being performed.</param>
        /// <param name="searchQuery">searchQuery to be provided by message extension.</param>
        /// <param name="count">Number of search results to return.</param>
        /// <param name="skip">Number of search results to skip.</param>
        /// <returns>List of search results.</returns>
        Task<IList<TicketEntity>> SearchTicketsAsync(TicketSearchScope searchScope, string languageCode, string searchQuery, int? count = null, int? skip = null);
    }
}
