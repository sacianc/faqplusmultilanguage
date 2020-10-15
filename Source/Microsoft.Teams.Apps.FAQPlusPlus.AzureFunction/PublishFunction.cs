// <copyright file="PublishFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure Function to publish QnA Maker knowledge base.
    /// </summary>
    public class PublishFunction
    {
        private readonly IEnumerable<IQnaServiceProvider> qnaServiceProviders;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly ISearchServiceDataProvider searchServiceDataProvider;
        private readonly IKnowledgeBaseSearchService knowledgeBaseSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishFunction"/> class.
        /// </summary>
        /// <param name="qnaServiceProviders">Language specific Question and answer maker service providers.</param>
        /// <param name="configurationProvider">Configuration service provider.</param>
        /// <param name="searchServiceDataProvider">Search service data provider.</param>
        /// <param name="knowledgeBaseSearchService">Knowledgebase search service.</param>
        public PublishFunction(IEnumerable<IQnaServiceProvider> qnaServiceProviders, IConfigurationDataProvider configurationProvider, ISearchServiceDataProvider searchServiceDataProvider, IKnowledgeBaseSearchService knowledgeBaseSearchService)
        {
            this.qnaServiceProviders = qnaServiceProviders;
            this.configurationProvider = configurationProvider;
            this.searchServiceDataProvider = searchServiceDataProvider;
            this.knowledgeBaseSearchService = knowledgeBaseSearchService;
        }

        /// <summary>
        /// Function to get and publish QnA Maker knowledge base.
        /// </summary>
        /// <param name="myTimer">Duration of publish operations.</param>
        /// <param name="log">Log.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("PublishFunction")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                // Get Languages supported by the bot from the app config.
                var languageQnAMakerKeyCombinationsJson = Environment.GetEnvironmentVariable("LanguageQnAMakerSubscriptionKeyJson");
                var languageQnAMakerKeyCombinations = JsonConvert.DeserializeObject<List<LanguageQnAMakerKeyCombination>>(languageQnAMakerKeyCombinationsJson);

                foreach (var languageQnaMakerKeyCombination in languageQnAMakerKeyCombinations)
                {
                    var languageKBConfigurationEntity = await this.configurationProvider.GetSavedLanguageKBConfigurationEntityAsync(languageQnaMakerKeyCombination.LanguageCode);

                    // Get applicable QnaServiceProvider for current applicable language.
                    var applicableQnaServiceProvider = this.qnaServiceProviders.FirstOrDefault(qsp => qsp.GetApplicableLanguageCode().Equals(languageQnaMakerKeyCombination.LanguageCode));
                    bool toBePublished = await applicableQnaServiceProvider.GetPublishStatusAsync(languageKBConfigurationEntity.KnowledgeBaseId).ConfigureAwait(false);
                    log.LogInformation("To be published - " + toBePublished);
                    log.LogInformation("knowledge base id - " + languageKBConfigurationEntity.KnowledgeBaseId);

                    if (toBePublished)
                    {
                        log.LogInformation("Publishing knowledge base");
                        await applicableQnaServiceProvider.PublishKnowledgebaseAsync(languageKBConfigurationEntity.KnowledgeBaseId).ConfigureAwait(false);
                    }

                    log.LogInformation("Setup azure search data");
                    await this.searchServiceDataProvider.SetupAzureSearchDataAsync(languageQnaMakerKeyCombination.LanguageCode, languageKBConfigurationEntity.KnowledgeBaseId).ConfigureAwait(false);

                    log.LogInformation("Update azure search service");
                    await this.knowledgeBaseSearchService.InitializeSearchServiceDependencyAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Exception occured while publishing knowledge base in QnA Maker.", SeverityLevel.Error);
                throw;
            }
        }
    }
}
