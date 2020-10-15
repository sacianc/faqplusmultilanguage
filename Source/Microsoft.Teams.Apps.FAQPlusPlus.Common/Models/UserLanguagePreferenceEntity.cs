// <copyright file="UserLanguagePreferenceEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// User language preference entity to store user's language preference for multi-language FAQ+.
    /// </summary>
    public class UserLanguagePreferenceEntity : TableEntity
    {
        /// <summary>
        /// UserLanguagePreference table store partition key name.
        /// </summary>
        public const string UserLanguagePreferencePartitionKey = "LanguagePreference";

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLanguagePreferenceEntity"/> class.
        /// </summary>
        public UserLanguagePreferenceEntity()
        {
            this.PartitionKey = UserLanguagePreferencePartitionKey;
        }

        /// <summary>
        /// Gets or sets user's Azure AD Identifier.
        /// </summary>
        public string UserObjectId { get; set; }

        /// <summary>
        /// Gets or sets language code for user's preferred language.
        /// </summary>
        public string LanguageCode { get; set; }
    }
}
