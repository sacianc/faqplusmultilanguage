// <copyright file="LanguageKBConfigurationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents language knowledgebase configuration entity used for storage and retrieval.
    /// </summary>
    public class LanguageKBConfigurationEntity : TableEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageKBConfigurationEntity"/> class.
        /// Default parameterlerss constructor.
        /// </summary>
        public LanguageKBConfigurationEntity()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageKBConfigurationEntity"/> class.
        /// </summary>
        /// <param name="languageCode">Language code corresponding to the knowledgebase configuration.</param>
        /// <param name="knowledgeBaseId">QnA maker knowledgebase id.</param>
        /// <param name="qnaMakerEndpointKey">QnA maker endpoint key.</param>
        /// <param name="teamId">Expert team id.</param>
        /// <param name="changeLanguageMessageText">Change language message text.</param>
        public LanguageKBConfigurationEntity(string languageCode, string knowledgeBaseId, string qnaMakerEndpointKey, string teamId, string changeLanguageMessageText)
        {
            this.PartitionKey = Constants.LanguageKBConfigurationPartitionKey;
            this.RowKey = languageCode;
            this.KnowledgeBaseId = knowledgeBaseId;
            this.QnaMakerEndpointKey = qnaMakerEndpointKey;
            this.TeamId = teamId;
            this.ChangeLanguageMessageText = changeLanguageMessageText;
        }

        /// <summary>
        /// Gets or sets Language specific QnA Knowledgebase Id which will be stored in table storage.
        /// </summary>
        [JsonProperty("KnowledgeBaseId")]
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets Language specific QnA Maker Endpoint key which will be stored in table storage.
        /// </summary>
        [JsonProperty("QnaMakerEndpointKey")]
        public string QnaMakerEndpointKey { get; set; }

        /// <summary>
        /// Gets or sets Language specific expert team id which will be stored in table storage.
        /// </summary>
        [JsonProperty("TeamId")]
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets change language message text which will be stored in table storage.
        /// </summary>
        [JsonProperty("ChangeLanguageMessageText")]
        public string ChangeLanguageMessageText { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        [JsonProperty("HelpTabText")]
        public string HelpTabText { get; set; }
    }
}
