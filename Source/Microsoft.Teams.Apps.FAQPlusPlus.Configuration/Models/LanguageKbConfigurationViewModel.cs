// <copyright file="LanguageKbConfigurationViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents langauge knowledgebase configuration view model to hold language specific configuration context for bot.
    /// </summary>
    public class LanguageKbConfigurationViewModel
    {
        /// <summary>
        /// Gets or sets language display name.
        /// </summary>
        public string LanguageDisplayName { get; set; }

        /// <summary>
        /// Gets or sets language code.
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// Gets or sets knowledge base id text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter knowledge base id.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "Knowledge base ID")]
        [RegularExpression(@"(\S)+", ErrorMessage = "Enter knowledge base ID which should not contain any whitespace.")]
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// Gets or sets team id textbox to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter team id.")]
        [MinLength(1)]
        [DataType(DataType.Text)]
        [Display(Name = "Team ID")]
        [RegularExpression(@"(\S)+", ErrorMessage = "Enter team id which should not contain any whitespace.")]
        public string TeamId { get; set; }

        /// <summary>
        /// Gets or sets welcome message text box to be used in View.
        /// </summary>
        [Required(ErrorMessage = "Enter a welcome message.")]
        [StringLength(maximumLength: 300, ErrorMessage = "Enter welcome message which should contain less than 300 characters.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Welcome message")]
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Gets or sets help tab message text box to be used in View
        /// </summary>
        [Required(ErrorMessage = "Enter help tab text.")]
        [StringLength(maximumLength: 3000, ErrorMessage = "Help tab text should contain less than 3000 characters.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Help tab text")]
        public string HelpTabText { get; set; }
    }
}