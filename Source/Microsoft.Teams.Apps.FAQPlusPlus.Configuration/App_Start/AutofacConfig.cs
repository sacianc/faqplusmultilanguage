// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Autofac configuration
    /// </summary>
    public static class AutofacConfig
    {
        /// <summary>
        /// Register Autofac dependencies
        /// </summary>
        /// <returns>Autofac container</returns>
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register(c => new ConfigurationDataProvider(
                 ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<IConfigurationDataProvider>()
                .SingleInstance();

            var qnaMakerClient = new QnAMakerClient(
                new ApiKeyServiceClientCredentials(
                ConfigurationManager.AppSettings["QnAMakerSubscriptionKey"]))
                { Endpoint = StripRouteFromQnAMakerEndpoint(ConfigurationManager.AppSettings["QnAMakerApiEndpointUrl"]) };

            builder.Register(c => qnaMakerClient)
                .As<IQnAMakerClient>()
                .SingleInstance();

            List<LanguageQnAMakerKeyCombination> languageQnAMakerKeyCombinations = JsonConvert.DeserializeObject<List<LanguageQnAMakerKeyCombination>>(ConfigurationManager.AppSettings["LanguageQnAMakerSubscriptionKeyJson"]);
            IEnumerable<IQnaServiceProvider> qnaServiceProviders = GetQnaServiceProviders(null, languageQnAMakerKeyCombinations);

            builder.Register(c => qnaServiceProviders)
                .As<IEnumerable<IQnaServiceProvider>>()
                .SingleInstance();

            var container = builder.Build();

            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }

        // Strip the route suffix from the endpoint
        private static string StripRouteFromQnAMakerEndpoint(string endpoint)
        {
            const string apiRoute = "/qnamaker/v4.0";

            if (endpoint.EndsWith(apiRoute, System.StringComparison.OrdinalIgnoreCase))
            {
                endpoint = endpoint.Substring(0, endpoint.Length - apiRoute.Length);
            }

            return endpoint;
        }

        private static IEnumerable<IQnaServiceProvider> GetQnaServiceProviders(IServiceProvider provider, List<LanguageQnAMakerKeyCombination> languageQnAMakerKeyCombinations)
         {
            var qnaServiceProviders = new List<QnaServiceProvider>();
            foreach (var languageQnAMakerKeyCombination in languageQnAMakerKeyCombinations)
            {
                IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(languageQnAMakerKeyCombination.QnAMakerSubscriptionKey)) { Endpoint = ConfigurationManager.AppSettings["QnAMakerApiEndpointUrl"] };
                string endpointKey = Task.Run(() => qnaMakerClient.EndpointKeys.GetKeysAsync()).Result.PrimaryEndpointKey;

                qnaServiceProviders.Add(new QnaServiceProvider(
                languageQnAMakerKeyCombination.LanguageCode,
                new ConfigurationDataProvider(ConfigurationManager.AppSettings["StorageConnectionString"]),
                null,
                qnaMakerClient,
                new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = ConfigurationManager.AppSettings["QnAMakerHostUrl"] }));
            }

            return qnaServiceProviders.AsEnumerable();
        }
    }

   
}