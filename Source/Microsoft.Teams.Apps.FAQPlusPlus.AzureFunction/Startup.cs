// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
using Newtonsoft.Json;
using IConfigurationDataProvider = Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.IConfigurationDataProvider;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction
{
    /// <summary>
    /// Azure function Startup Class.
    /// </summary>
    public class Startup : IWebJobsStartup
    {
        /// <summary>
        /// Application startup configuration.
        /// </summary>
        /// <param name="builder">Webjobs builder.</param>
        public void Configure(IWebJobsBuilder builder)
        {
            // Get Languages supported by the bot from the app config.
            var languageQnAMakerKeyCombinationsJson = Environment.GetEnvironmentVariable("LanguageQnAMakerSubscriptionKeyJson");
            var languageQnAMakerKeyCombinations = JsonConvert.DeserializeObject<List<LanguageQnAMakerKeyCombination>>(languageQnAMakerKeyCombinationsJson);

            // Initialize Qna service providers for each language supported by the bot.
            builder.Services.AddSingleton<IEnumerable<IQnaServiceProvider>>(
                (provider) => this.GetQnaServiceProviders(provider, languageQnAMakerKeyCombinations));
            builder.Services.AddSingleton<IConfigurationDataProvider, Common.Providers.ConfigurationDataProvider>();
            builder.Services.AddSingleton<ISearchServiceDataProvider>((provider) => new SearchServiceDataProvider(provider.GetRequiredService<IEnumerable<IQnaServiceProvider>>(), Environment.GetEnvironmentVariable("StorageConnectionString")));
            builder.Services.AddSingleton<IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(Environment.GetEnvironmentVariable("StorageConnectionString")));
            builder.Services.AddSingleton<IKnowledgeBaseSearchService, KnowledgeBaseSearchService>();
            builder.Services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(Environment.GetEnvironmentVariable("SearchServiceName"), Environment.GetEnvironmentVariable("SearchServiceQueryApiKey"), Environment.GetEnvironmentVariable("SearchServiceAdminApiKey"), Environment.GetEnvironmentVariable("StorageConnectionString")));
        }

        private IEnumerable<IQnaServiceProvider> GetQnaServiceProviders(IServiceProvider provider, List<LanguageQnAMakerKeyCombination> languageQnAMakerKeyCombinations)
        {
            var qnaServiceProviders = new List<QnaServiceProvider>();
            foreach (var languageQnAMakerKeyCombination in languageQnAMakerKeyCombinations)
            {
                IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(languageQnAMakerKeyCombination.QnAMakerSubscriptionKey)) { Endpoint = Environment.GetEnvironmentVariable("QnAMakerApiUrl") };

                qnaServiceProviders.Add(new QnaServiceProvider(
                    languageQnAMakerKeyCombination.LanguageCode,
                    provider.GetRequiredService<IConfigurationDataProvider>(),
                    provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(),
                    qnaMakerClient));
            }

            return qnaServiceProviders.AsEnumerable();
        }
    }
}
