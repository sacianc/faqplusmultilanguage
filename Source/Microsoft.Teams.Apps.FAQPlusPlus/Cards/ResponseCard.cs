// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// Construct the response card - when user asks a question to QnA Maker through bot.
        /// </summary>
        /// <param name="answer">Knowledgebase answer, from QnA Maker service.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <param name="knowledgeBaseQuestion">Knowledgebase question, from QnQ Maker service.</param>
        /// <returns>Response card.</returns>
        public static Attachment GetCard(string answer, string userQuestion, string knowledgeBaseQuestion)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = knowledgeBaseQuestion,
                        Wrap = true,
                        Size = AdaptiveTextSize.Medium,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.AskAnExpertButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.AskAnExpertDisplayText,
                                Text = Constants.AskAnExpert,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                            KnowledgeBaseQuestion = knowledgeBaseQuestion,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.ShareFeedbackButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.ShareFeedbackDisplayText,
                                Text = Constants.ShareFeedback,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                            KnowledgeBaseQuestion = knowledgeBaseQuestion,
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        /// <summary>
        /// Construct the response card with multi turn prompts - when user asks a question to QnA Maker through bot.
        /// </summary>
        /// <param name="question">Knowledgebase question, from QnA Maker service.</param>
        /// <param name="answer">Knowledgebase answer, from QnA Maker service.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <param name="prompts">Prompts associated with the current question.</param>
        /// <param name="questionId">Knowledgebase question id, from QnA Maker service.</param>
        /// <returns>Response card.</returns>
        public static Attachment GetCardWithPrompts(string question, string answer, string userQuestion, IList<PromptDTO> prompts, int questionId)
        {
            List<AdaptiveAction> adaptiveSubmitAction = new List<AdaptiveAction>()
            {
                new AdaptiveSubmitAction
                {
                    Title = Strings.AskAnExpertButtonText,
                    Data = new ResponseCardPayload
                    {
                        MsTeams = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            DisplayText = Strings.AskAnExpertDisplayText,
                            Text = Constants.AskAnExpert,
                        },
                        UserQuestion = userQuestion,
                        KnowledgeBaseAnswer = answer,
                    },
                },
                new AdaptiveSubmitAction
                {
                    Title = Strings.ShareFeedbackButtonText,
                    Data = new ResponseCardPayload
                    {
                        MsTeams = new CardAction
                        {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.ShareFeedbackDisplayText,
                                Text = Constants.ShareFeedback,
                        },
                        UserQuestion = userQuestion,
                        KnowledgeBaseAnswer = answer,
                    },
                },
                new AdaptiveShowCardAction
                {
                    Title = Strings.ShowPromptsTitleText,
                    Card = new AdaptiveCard("1.0")
                    {
                        Actions = AddPrompts(prompts, questionId, userQuestion, 0, 2),
                    },
                },
            };

            AdaptiveCard responseCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Strings.ResponseHeaderText,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = question,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true,
                    },
                },
                Actions = adaptiveSubmitAction,
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        /// <summary>
        /// Construct show card containing prompt values.
        /// </summary>
        /// <param name="prompts">Prompts associated with the current question.</param>
        /// <param name="questionId">questionId of the question.</param>
        /// <param name="userQuestion">User query asked as question.</param>
        /// <param name="startIndex">Starting index of values in prompts array.</param>
        /// <param name="maxPrompts">Maximum number of prompts allowed in one show card.</param>
        /// <returns>Response card.</returns>
        private static List<AdaptiveAction> AddPrompts(IList<PromptDTO> prompts, int questionId, string userQuestion, int startIndex, int maxPrompts)
        {
            List<AdaptiveAction> adaptiveSubmitAction = new List<AdaptiveAction>();
            int currentIndex = 0;
            int maxIndex = startIndex + maxPrompts > prompts.Count ? prompts.Count : startIndex + maxPrompts;
            for (int i = startIndex; i < maxIndex; i++)
            {
                currentIndex = i;
                AdaptiveSubmitAction promptAction = new AdaptiveSubmitAction
                {
                    Title = prompts[i].DisplayText,
                    Data = new ResponseCardPayload
                    {
                        MsTeams = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            DisplayText = prompts[i].DisplayText,
                            Text = prompts[i].DisplayText,
                        },
                        UserQuestion = prompts[i].DisplayText,
                        KnowledgeBaseAnswer = prompts[i].DisplayText,
                        QnAId = (int)prompts[i].QnaId,
                        PreviousQuestionId = questionId,
                        PreviousUserQuery = userQuestion,
                    },
                };
                adaptiveSubmitAction.Add(promptAction);
            }

            if (prompts.Count - startIndex > maxPrompts)
            {
                adaptiveSubmitAction.Add(
                    new AdaptiveShowCardAction
                    {
                        Title = Strings.MoreTitleText,
                        Card = new AdaptiveCard("1.0")
                        {
                            Actions = AddPrompts(prompts, questionId, userQuestion, currentIndex + 1, maxPrompts),
                        },
                    });
            }

            return adaptiveSubmitAction;
        }
    }
}