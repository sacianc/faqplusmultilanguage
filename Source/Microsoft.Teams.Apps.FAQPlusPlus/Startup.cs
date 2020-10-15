// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.AzureAD.UI;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Web;
    using Microsoft.Identity.Web.Client.TokenCacheProviders;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This a Startup class for this Bot.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Startup Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets Configurations Interfaces.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Hosting Environment.</param>
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"> Service Collection Interface.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.AddControllersWithViews();

            // Create the telemetry middleware(used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            services.AddMemoryCache();
            services.AddSingleton<IMemoryCache, MemoryCache>();

            //var scopes = this.Configuration["AzureAd:GraphScope"].Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            services.Configure<AzureADOptions>(this.Configuration.GetSection("AzureAd"));
            services.AddProtectWebApiWithMicrosoftIdentityPlatformV2(this.Configuration)
                  .AddProtectedApiCallsWebApis(this.Configuration)
                   .AddInMemoryTokenCaches();
            services.AddAuthorization();

            services.Configure<KnowledgeBaseSettings>(knowledgeBaseSettings =>
            {
                knowledgeBaseSettings.SearchServiceName = this.Configuration["SearchServiceName"];
                knowledgeBaseSettings.SearchServiceQueryApiKey = this.Configuration["SearchServiceQueryApiKey"];
                knowledgeBaseSettings.SearchServiceAdminApiKey = this.Configuration["SearchServiceAdminApiKey"];
                knowledgeBaseSettings.SearchIndexingIntervalInMinutes = this.Configuration["SearchIndexingIntervalInMinutes"];
                knowledgeBaseSettings.StorageConnectionString = this.Configuration["StorageConnectionString"];
            });

            services.Configure<QnAMakerSettings>(qnAMakerSettings =>
            {
                qnAMakerSettings.ScoreThreshold = this.Configuration["ScoreThreshold"];
            });

            services.Configure<BotSettings>(botSettings =>
            {
                botSettings.AccessCacheExpiryInDays = Convert.ToInt32(this.Configuration["AccessCacheExpiryInDays"]);
                botSettings.AppBaseUri = this.Configuration["AppBaseUri"];
                botSettings.MicrosoftAppId = this.Configuration["MicrosoftAppId"];
                botSettings.TenantId = this.Configuration["TenantId"];
            });

            services.Configure<LanguageQnAMakerSubscriptionKeySettings>(languageQnAMakerSubscriptionKeySettings =>
            {
                var languageQnAMakerKeyCombinationsSection = this.Configuration.GetSection("LanguageQnAMakerSubscriptionKeyJson");
                languageQnAMakerSubscriptionKeySettings.LanguageQnAMakerKeyCombinations = languageQnAMakerKeyCombinationsSection.Get<List<LanguageQnAMakerKeyCombination>>();
            });

            // Init configuration settings.
            var configurationSettings = new ConfigurationSettings();
            this.Configuration.Bind(configurationSettings);
            services.AddSingleton(configurationSettings);

            services.AddSingleton<Common.Providers.IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(this.Configuration["StorageConnectionString"]));
            services.AddHttpClient();
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<ITicketsProvider>(new TicketsProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
            services.AddSingleton(new MicrosoftAppCredentials(this.Configuration["MicrosoftAppId"], this.Configuration["MicrosoftAppPassword"]));

            // Get Languages supported by the bot from the app config.
            var languageQnAMakerKeyCombinationsSection = this.Configuration.GetSection("LanguageQnAMakerSubscriptionKeyJson");
            var languageQnAMakerKeyCombinations = languageQnAMakerKeyCombinationsSection.Get<List<LanguageQnAMakerKeyCombination>>();

            // Initialize Qna service providers for each language supported by the bot.
            services.AddSingleton<IEnumerable<IQnaServiceProvider>>(
                (provider) => this.GetQnaServiceProviders(provider, languageQnAMakerKeyCombinations));
            services.AddSingleton<IActivityStorageProvider>((provider) => new ActivityStorageProvider(provider.GetRequiredService<IOptionsMonitor<KnowledgeBaseSettings>>()));
            services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(this.Configuration["SearchServiceName"], this.Configuration["SearchServiceQueryApiKey"], this.Configuration["SearchServiceAdminApiKey"], this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IUserLanguagePreferenceProvider>((provider) => new UserLanguagePreferenceProvider(this.Configuration["StorageConnectionString"]));

            services.AddSingleton<ISearchService, SearchService>();
            services.AddTransient(sp => (BotFrameworkAdapter)sp.GetRequiredService<IBotFrameworkHttpAdapter>());
            services.AddTransient<IBot, FaqPlusPlusBot>();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });
        }

        private IEnumerable<IQnaServiceProvider> GetQnaServiceProviders(IServiceProvider provider, List<LanguageQnAMakerKeyCombination> languageQnAMakerKeyCombinations)
        {
            var qnaServiceProviders = new List<QnaServiceProvider>();
            foreach (var languageQnAMakerKeyCombination in languageQnAMakerKeyCombinations)
            {
                IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(languageQnAMakerKeyCombination.QnAMakerSubscriptionKey)) { Endpoint = this.Configuration["QnAMakerApiEndpointUrl"] };
                string endpointKey = Task.Run(() => qnaMakerClient.EndpointKeys.GetKeysAsync()).Result.PrimaryEndpointKey;

                qnaServiceProviders.Add(new QnaServiceProvider(
                  languageQnAMakerKeyCombination.LanguageCode,
                  provider.GetRequiredService<Common.Providers.IConfigurationDataProvider>(),
                  provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(),
                  qnaMakerClient,
                  new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = languageQnAMakerKeyCombination.QnAMakerHostUrl }));
            }

            return qnaServiceProviders.AsEnumerable();
        }
    }
}