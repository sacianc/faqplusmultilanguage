// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Home Controller
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly IEnumerable<IQnaServiceProvider> qnaServiceProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configurationPovider">configurationPovider dependency injection.</param>
        /// <param name="qnaServiceProviders">qnaServiceProviders dependency injection.</param>
        public HomeController(IConfigurationDataProvider configurationPovider, IEnumerable<IQnaServiceProvider> qnaServiceProviders)
        {
            this.configurationProvider = configurationPovider;
            this.qnaServiceProviders = qnaServiceProviders;
        }

        /// <summary>
        /// The landing page.
        /// </summary>
        /// <returns>Default landing page view.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            var languageQnAMakerSubscriptionKeyJson = ConfigurationManager.AppSettings["LanguageQnAMakerSubscriptionKeyJson"];
            var languageQnAMakerKeyCombinations = JsonConvert.DeserializeObject<List<LanguageQnAMakerKeyCombination>>(languageQnAMakerSubscriptionKeyJson);
            List<LanguageKbConfigurationViewModel> langKbLst = new List<LanguageKbConfigurationViewModel>();
            string languages = string.Empty;
            for (int i = 0; i < languageQnAMakerKeyCombinations.Count; i++)
            {
                LanguageKbConfigurationViewModel langKb = new LanguageKbConfigurationViewModel();
                langKb.LanguageCode = languageQnAMakerKeyCombinations[i].LanguageCode;
                langKbLst.Add(langKb);
                if (languages != string.Empty)
                {
                    languages = languages + "," + langKb.LanguageCode;
                }
                else
                {
                    languages = langKb.LanguageCode;
                }
            }

            this.ViewBag.Languages = languages;

            return this.View(langKbLst);
        }

        /// <summary>
        /// GetSavedDetailsAsync
        /// </summary>
        /// <param name="language">Language Code</param>
        /// <returns>Json Data</returns>
        public async Task<JsonResult> GetSavedDetailsAsync(string language)
        {
            var jsonData = this.Json(await this.configurationProvider.GetSavedLanguageKBConfigurationEntityAsync(language).ConfigureAwait(true));

            return this.Json(new { data = jsonData }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Parse team id from first and then proceed to save it on success.
        /// </summary>
        /// <param name="item">Model data</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveLanguageKbConfigurationDetails(LanguageKbConfigurationViewModel item)
        {
            string teamIdAfterParse = ParseTeamIdFromDeepLink(item.TeamId ?? string.Empty);
            if (string.IsNullOrWhiteSpace(teamIdAfterParse))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The provided team id is not valid.");
            }
            else
            {
                // Get bot supported languages from web.config.
                var languageQnAMakerSubscriptionKeyJson = ConfigurationManager.AppSettings["LanguageQnAMakerSubscriptionKeyJson"];
                var languageQnAMakerKeyCombinations = JsonConvert.DeserializeObject<List<LanguageQnAMakerKeyCombination>>(languageQnAMakerSubscriptionKeyJson);
                var supportedLanguages = from languageQnAMakerKeyCombination in languageQnAMakerKeyCombinations
                                         select new BotLanguageDetail { Code = languageQnAMakerKeyCombination.LanguageCode, Default = languageQnAMakerKeyCombination.Default, Name = languageQnAMakerKeyCombination.LanguageName };

                // Save the bot supported language details to table storage.
                await this.configurationProvider.UpsertEntityAsync(JsonConvert.SerializeObject(supportedLanguages), ConfigurationEntityTypes.SupportedLanguagesKey).ConfigureAwait(false);

                LanguageQnAMakerKeyCombination langQnaDetails = languageQnAMakerKeyCombinations.FirstOrDefault(x => x.LanguageCode == item.LanguageCode);
                IQnaServiceProvider applicableQnaServiceProvider = this.qnaServiceProviders.FirstOrDefault(qsp => qsp.GetApplicableLanguageCode().Equals(item.LanguageCode));
                bool isValidKnowledgeBaseId = await applicableQnaServiceProvider.IsKnowledgeBaseIdValid(item.KnowledgeBaseId).ConfigureAwait(false);
                if (isValidKnowledgeBaseId)
                {
                    LanguageKBConfigurationEntity langKBConfigEntity = new LanguageKBConfigurationEntity();
                    langKBConfigEntity.TeamId = teamIdAfterParse;
                    langKBConfigEntity.KnowledgeBaseId = item.KnowledgeBaseId;
                    langKBConfigEntity.ChangeLanguageMessageText = item.WelcomeMessage;
                    langKBConfigEntity.HelpTabText = item.HelpTabText;
                    langKBConfigEntity.PartitionKey = "LanguageKBConfiguration";
                    langKBConfigEntity.QnaMakerEndpointKey = langQnaDetails.QnAMakerSubscriptionKey;
                    langKBConfigEntity.RowKey = item.LanguageCode;
                    bool isSaved = await this.configurationProvider.UpsertLanguageKBConfigurationEntityAsync(item.LanguageCode, langKBConfigEntity);
                    if (isSaved)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.OK);
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the team id due to an internal error. Try again.");
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The provided knowledgebase id is not valid.");
                }
            }
        }

        /// <summary>
        /// Save or update teamId in table storage which is received from View.
        /// </summary>
        /// <param name="teamId">Team id is the unique deep link URL string associated with each team.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpsertTeamIdAsync(string teamId)
        {
            bool isSaved = await this.configurationProvider.UpsertEntityAsync(teamId, ConfigurationEntityTypes.TeamId).ConfigureAwait(false);
            if (isSaved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the team id due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved team id from table storage.
        /// </summary>
        /// <returns>Team id.</returns>
        [HttpGet]
        public async Task<string> GetSavedTeamIdAsync()
        {
            return await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).ConfigureAwait(false);
        }

        /// <summary>
        /// Save or update knowledgeBaseId in table storage which is received from View.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string to identify knowledgebase.</param>
        /// <returns>View</returns>
        [HttpGet]
        public async Task<ActionResult> UpsertKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            bool isSaved = await this.configurationProvider.UpsertEntityAsync(knowledgeBaseId, ConfigurationEntityTypes.KnowledgeBaseId).ConfigureAwait(false);
            if (isSaved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the knowledge base id due to an internal error. Try again.");
            }
        }
               
        /// <summary>
        /// Get already saved knowledgebase id from table storage.
        /// </summary>
        /// <returns>knowledgebase id.</returns>
        [HttpGet]
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            return await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId).ConfigureAwait(false);
        }

        /// <summary>
        /// Save or update welcome message to be used by bot in table storage which is received from View.
        /// </summary>
        /// <param name="welcomeMessage">Welcome message text to show once the user install the bot for the very first time.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveWelcomeMessageAsync(string welcomeMessage)
        {
            bool isSaved = await this.configurationProvider.UpsertEntityAsync(welcomeMessage, ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
            if (isSaved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the welcome message due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved Welcome message from table storage.
        /// </summary>
        /// <returns>Welcome message.</returns>
        public async Task<string> GetSavedWelcomeMessageAsync()
        {
            var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(welcomeText))
            {
                await this.SaveWelcomeMessageAsync(Strings.DefaultWelcomeMessage).ConfigureAwait(false);
            }

            return await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
        }

        /// <summary>
        /// Save or update help tab text to be used by bot in table storage which is received from View.
        /// </summary>
        /// <param name="helpTabText">help tab text.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveHelpTabTextAsync(string helpTabText)
        {
            bool saved = await this.configurationProvider.UpsertEntityAsync(helpTabText, ConfigurationEntityTypes.HelpTabText).ConfigureAwait(false);
            if (saved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the help tab text due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved help tab message from table storage.
        /// </summary>
        /// <returns>Help tab text.</returns>
        public async Task<string> GetSavedHelpTabTextAsync()
        {
            var helpText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.HelpTabText).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(helpText))
            {
                await this.SaveHelpTabTextAsync(Strings.DefaultHelpTabText).ConfigureAwait(false);
            }

            return await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.HelpTabText).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets list for languages suported by the bot.
        /// </summary>
        /// <returns>List of <see cref="BotLanguageDetail"/></returns>
        public async Task<List<BotLanguageDetail>> GetBotSupportedLanguages()
        {
            var supportedLanguagesJson = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.SupportedLanguagesKey).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<List<BotLanguageDetail>>(supportedLanguagesJson);
        }

        /// <summary>
        /// Gets language specific knowledgebase and team details.
        /// </summary>
        /// <param name="languageCode">Language code.</param>
        /// <param name="languageName">Language Name.</param>
        /// <returns>List of <see cref="LanguageKbConfigurationViewModel"/></returns>
        public async Task<LanguageKbConfigurationViewModel> GetLanguageKbDetails(string languageCode, string languageName)
        {
            var languageKbConfigObject = await this.configurationProvider.GetSavedLanguageKBConfigurationEntityAsync(languageCode).ConfigureAwait(false);
            LanguageKbConfigurationViewModel languageKbDetail = new LanguageKbConfigurationViewModel() { LanguageCode = languageCode, LanguageDisplayName = languageName, KnowledgeBaseId = languageKbConfigObject.KnowledgeBaseId, TeamId = languageKbConfigObject.TeamId, WelcomeMessage = languageKbConfigObject.ChangeLanguageMessageText };
            return languageKbDetail;
        }

        /// <summary>
        /// Based on deep link URL received find team id and return it to that it can be saved.;
        /// </summary>
        /// <param name="teamIdDeepLink">team id deep link.</param>
        /// <returns>team id decoded string.</returns>
        private static string ParseTeamIdFromDeepLink(string teamIdDeepLink)
        {
            // team id regex match
            // for a pattern like https://teams.microsoft.com/l/team/19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2/conversations?groupId=53b4782c-7c98-4449-993a-441870d10af9&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47
            // regex checks for 19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2
            var match = Regex.Match(teamIdDeepLink, @"teams.microsoft.com/l/team/(\S+)/");

            if (!match.Success)
            {
                return string.Empty;
            }

            return HttpUtility.UrlDecode(match.Groups[1].Value);
        }
    }

}