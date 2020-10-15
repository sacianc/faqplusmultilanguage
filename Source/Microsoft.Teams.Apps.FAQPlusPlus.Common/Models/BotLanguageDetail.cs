// <copyright file="BotLanguageDetail.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Bot supported language details.
    /// </summary>
    public class BotLanguageDetail
    {
        /// <summary>
        /// Gets or sets display name for the language.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets language code.
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is default language for the bot.
        /// </summary>
        [JsonProperty("default")]
        public bool Default { get; set; }
    }
}
