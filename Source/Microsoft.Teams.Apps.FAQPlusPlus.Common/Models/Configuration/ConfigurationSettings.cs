// <copyright file="ConfigurationSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration
{
    /// <summary>
    /// The class represents the values defined in the appsettings.json.
    /// The application binds an instance of the class to the appsettings.json values.
    /// </summary>
    public class ConfigurationSettings
    {
        /// <summary>
        /// Gets or sets the ServiceBusConnection value.
        /// </summary>
        public string ServiceBusConnection { get; set; }

        /// <summary>
        /// Gets or sets the StorageConnectionString value.
        /// </summary>
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets MicrosoftAppId value. It is the bot id.
        /// </summary>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets MicrosoftAppPassword value. It is the bot password.
        /// </summary>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Gets or sets RetryCount value.
        /// It is used by HttpClient in sending bot proactive message.
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets RetryCountOnThrottling value.
        /// It is used by HttpClient in sending bot proactive message.
        /// </summary>
        public int RetryCountOnThrottling { get; set; }

        /// <summary>
        /// Gets or sets RetryDelay value.
        /// It is used by HttpClient in sending bot proactive message.
        /// </summary>
        public int RetryDelay { get; set; }

        /// <summary>
        /// Gets or sets RetryDelayOnThrottling value.
        /// It is used by HttpClient in sending bot proactive message.
        /// </summary>
        public int RetryDelayOnThrottling { get; set; }

        /// <summary>
        /// Gets or sets BaseUrl value.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets ManifestAppId value.
        /// </summary>
        public string ManifestAppId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tenant validation is turned off or not.
        /// </summary>
        public bool DisableTenantFilter { get; set; }

        /// <summary>
        /// Gets or sets AllowedTenants value.
        /// </summary>
        public string AllowedTenants { get; set; }
    }
}
