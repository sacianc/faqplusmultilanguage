// <copyright file="SmeTicketCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Represents an SME ticket used for both in place card update activity within SME channel
    /// when changing the ticket status and notification card when bot posts user question to SME channel.
    /// </summary>
    public class SmeTicketCard
    {
        private readonly TicketEntity ticket;
        private readonly UpdateTicketResponsePayload updateTicketResponsePayload;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeTicketCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        /// <param name="payload">Payload from the response card.</param>
        public SmeTicketCard(TicketEntity ticket, UpdateTicketResponsePayload payload)
        {
            this.ticket = ticket;
            this.updateTicketResponsePayload = new UpdateTicketResponsePayload();
            if (payload != null)
            {
                this.updateTicketResponsePayload = payload;
            }
        }

        /// <summary>
        /// Gets the ticket that is the basis for the information in this card.
        /// </summary>
        protected TicketEntity Ticket => this.ticket;

        /// <summary>
        /// Returns an attachment based on the state and information of the ticket.
        /// </summary>
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <param name="appBaseUri">Application base uri.</param>
        /// <param name="showValidationErrors">Determines whether we show validation errors.</param>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment(DateTimeOffset? localTimestamp, string appBaseUri, bool showValidationErrors)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = string.Format(CultureInfo.InvariantCulture, Strings.SMETicketHeaderText, this.Ticket.TicketId, this.Ticket.Title),
                        Size = AdaptiveTextSize.Medium,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true,
                    },
                },
            };

            if (this.ticket.KnowledgeBaseQuestion != null)
            {
                card.Body.Add(new AdaptiveTextBlock
                    {
                        Text = string.Format(CultureInfo.InvariantCulture, Strings.SMETicketActualQuestionInKBText, this.Ticket.KnowledgeBaseQuestion),
                        Wrap = true,
                        Size = AdaptiveTextSize.Small,
                        Spacing = AdaptiveSpacing.None,
                    });
            }
            else
            {
                card.Body.Add(new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveImage
                                            {
                                                Url = new Uri(string.Format("{0}/content/RedInfoIcon.png", appBaseUri)),
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                                AltText = "Info icon",
                                            },
                                        },
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = Strings.SMETicketNoQuestionInKBText,
                                                Wrap = true,
                                                Size = AdaptiveTextSize.Small,
                                                Spacing = AdaptiveSpacing.None,
                                            },
                                        },
                                    },
                                },
                });
            }

            if (this.Ticket.Status == (int)TicketState.Answered)
            {
                card.Body.Add(new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "80px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.StatusFactTitle,
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Spacing = AdaptiveSpacing.Medium,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "**" + CardHelper.GetTicketDisplayStatusForSme(this.Ticket, this.updateTicketResponsePayload) + "**",
                                        Color = AdaptiveTextColor.Good,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                });
            }
            else
            {
                card.Body.Add(new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "80px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.StatusFactTitle,
                                        Wrap = true,
                                        Spacing = AdaptiveSpacing.None,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Spacing = AdaptiveSpacing.Medium,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = "**" + CardHelper.GetTicketDisplayStatusForSme(this.Ticket, this.updateTicketResponsePayload) + "**",
                                        Color = AdaptiveTextColor.Attention,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                });
            }

            card.Body.Add(new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "80px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.DescriptionFact,
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Spacing = AdaptiveSpacing.Medium,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = this.Ticket.Description,
                                        Wrap = true,
                                        MaxLines = 3,
                                    },
                                },
                            },
                        },
            });

            card.Body.Add(new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "80px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.QuestionAskedByFactTitle,
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Spacing = AdaptiveSpacing.Medium,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = this.Ticket.RequesterName,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
            });

            card.Body.Add(new AdaptiveColumnSet
            {
                Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "80px",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.DateFactTitle,
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Spacing = AdaptiveSpacing.Medium,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = CardHelper.GetFormattedDateInUserTimeZone(this.Ticket.DateCreated, localTimestamp),
                                        Wrap = true,
                                    },
                                },
                            },
                        },
            });

            if (!string.IsNullOrEmpty(this.Ticket.KnowledgeBaseQuestion))
            {
                card.Body.Add(new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "80px",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = Strings.ExistingAnswerFactTitle,
                                    Wrap = true,
                                },
                            },
                        },
                        new AdaptiveColumn
                        {
                            Width = "auto",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveChoiceSetInput
                                {
                                    Id = Strings.ExistingAnswerFactTitle,
                                    IsMultiSelect = false,
                                    Style = AdaptiveChoiceInputStyle.Compact,
                                    Value = Strings.ExistingAnswerFactTitle,
                                    Choices = new List<AdaptiveChoice>
                                    {
                                        new AdaptiveChoice
                                        {
                                            Title = this.Ticket.KnowledgeBaseAnswer,
                                            Value = Strings.ExistingAnswerFactTitle,
                                        },
                                    },
                                },
                            },
                        },
                    },
                });
            }

            if (this.ticket.AnswerBySME == null && !showValidationErrors)
            {
                card.Actions = this.BuildActions(appBaseUri);
            }
            else
            {
                if (this.updateTicketResponsePayload.Action == UpdateTicketResponsePayload.RespondAction)
                {
                    card.Body.Add(
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Width = AdaptiveColumnWidth.Auto,
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = Strings.SMETicketCardYourAnswerLabel,
                                            Wrap = true,
                                            Size = AdaptiveTextSize.Small,
                                        },
                                    },
                                },
                                new AdaptiveColumn
                                {
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = (showValidationErrors && string.IsNullOrWhiteSpace(this.updateTicketResponsePayload.Answer)) ? Strings.MandatoryAnswerFieldText : string.Empty,
                                            Color = AdaptiveTextColor.Attention,
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                            Wrap = true,
                                        },
                                    },
                                },
                            },
                        });
                    card.Body.Add(new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(UpdateTicketResponsePayload.AnswerForRespond),
                        Placeholder = Strings.ResponsePlaceholderText,
                        IsMultiline = true,
                        Value = this.updateTicketResponsePayload.AnswerForRespond,
                    });

                    card.Body.Add(new AdaptiveChoiceSetInput
                    {
                        Id = nameof(UpdateTicketResponsePayload.AddorAppendAction),
                        IsMultiSelect = true,
                        Style = AdaptiveChoiceInputStyle.Expanded,
                        Value = this.updateTicketResponsePayload.AddorAppendAction,
                        Choices = new List<AdaptiveChoice>
                            {
                                new AdaptiveChoice
                                {
                                    Title = Strings.AppendToKBText,
                                    Value = Strings.AppendActionMessage,
                                },
                            },
                    });

                    if (showValidationErrors)
                    {
                        card.Actions.Add(new AdaptiveSubmitAction
                        {
                            Title = Strings.SubmitResponseButtonText,
                            Data = new UpdateTicketResponsePayload
                            {
                                TicketId = this.Ticket.TicketId,
                                Action = UpdateTicketResponsePayload.RespondAction,
                            },
                        });
                    }
                }
                else if (this.updateTicketResponsePayload.Action == UpdateTicketResponsePayload.AddRespondAction)
                {
                    card.Body.Add(
                        new AdaptiveColumnSet
                        {
                            Columns = new List<AdaptiveColumn>
                            {
                                new AdaptiveColumn
                                {
                                    Width = AdaptiveColumnWidth.Auto,
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = Strings.SMETicketCardYourAnswerLabel,
                                            Wrap = true,
                                            Size = AdaptiveTextSize.Small,
                                        },
                                    },
                                },
                                new AdaptiveColumn
                                {
                                    Items = new List<AdaptiveElement>
                                    {
                                        new AdaptiveTextBlock
                                        {
                                            Text = (showValidationErrors && string.IsNullOrWhiteSpace(this.updateTicketResponsePayload.AnswerForRespond)) ? Strings.MandatoryAnswerFieldText : string.Empty,
                                            Color = AdaptiveTextColor.Attention,
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                            Wrap = true,
                                        },
                                    },
                                },
                            },
                        });
                    card.Body.Add(new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(UpdateTicketResponsePayload.Answer),
                        Placeholder = Strings.ResponsePlaceholderText,
                        IsMultiline = true,
                        Value = this.updateTicketResponsePayload.Answer,
                    });

                    card.Body.Add(new AdaptiveChoiceSetInput
                    {
                        Id = nameof(UpdateTicketResponsePayload.AddorAppendAction),
                        IsMultiSelect = true,
                        Style = AdaptiveChoiceInputStyle.Expanded,
                        Value = this.updateTicketResponsePayload.AddorAppendAction,
                        Choices = new List<AdaptiveChoice>
                            {
                                new AdaptiveChoice
                                {
                                    Title = Strings.AddToKBText,
                                    Value = Strings.AddActionMessage,
                                },
                            },
                    });

                    if (showValidationErrors)
                    {
                        card.Actions.Add(new AdaptiveSubmitAction
                        {
                            Title = Strings.SubmitResponseButtonText,
                            Data = new UpdateTicketResponsePayload
                            {
                                TicketId = this.Ticket.TicketId,
                                Action = UpdateTicketResponsePayload.AddRespondAction,
                            },
                        });
                    }
                }
                else
                {
                    card.Body.Add(new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.UpdateExistingButtonText,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = (showValidationErrors && string.IsNullOrWhiteSpace(this.updateTicketResponsePayload.Answer)) ? Strings.MandatoryAnswerFieldText : string.Empty,
                                        Color = AdaptiveTextColor.Attention,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    });
                    card.Body.Add(new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(UpdateTicketResponsePayload.Answer),
                        Placeholder = Strings.ResponsePlaceholderText,
                        IsMultiline = true,
                        Value = this.updateTicketResponsePayload.Answer,
                    });
                    card.Body.Add(new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(string.Format("{0}/content/RedInfoIcon.png", appBaseUri)),
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                        AltText = "Info icon",
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = Strings.WarningMessageOnUpdateAnswer,
                                        Wrap = true,
                                        Size = AdaptiveTextSize.Small,
                                        Spacing = AdaptiveSpacing.None,
                                    },
                                },
                            },
                        },
                    });

                    if (showValidationErrors)
                    {
                        card.Actions.Add(new AdaptiveSubmitAction
                        {
                            Title = Strings.SubmitResponseButtonText,
                            Data = new UpdateTicketResponsePayload
                            {
                                TicketId = this.Ticket.TicketId,
                                Action = UpdateTicketResponsePayload.UpdateResponseAction,
                            },
                        });
                    }
                }
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Return the appropriate set of card actions based on the state and information in the ticket.
        /// </summary>
        /// <param name="appBaseUri">Application base uri.</param>
        /// <returns>Adaptive card actions.</returns>
        protected virtual List<AdaptiveAction> BuildActions(string appBaseUri)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();

            if (string.IsNullOrEmpty(this.Ticket.KnowledgeBaseAnswer))
            {
                actionsList.Add(new AdaptiveShowCardAction
                {
                    Title = Strings.RespondButtonText,
                    Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = Strings.SMETicketCardYourAnswerLabel,
                                Wrap = true,
                                Size = AdaptiveTextSize.Small,
                            },
                            new AdaptiveTextInput
                            {
                                Spacing = AdaptiveSpacing.Small,
                                Id = nameof(UpdateTicketResponsePayload.Answer),
                                Placeholder = Strings.ResponsePlaceholderText,
                                IsMultiline = true,
                                Value = this.updateTicketResponsePayload.Answer,
                            },
                            new AdaptiveChoiceSetInput
                            {
                                Id = nameof(UpdateTicketResponsePayload.AddorAppendAction),
                                IsMultiSelect = true,
                                Style = AdaptiveChoiceInputStyle.Expanded,
                                Choices = new List<AdaptiveChoice>
                                        {
                                            new AdaptiveChoice
                                            {
                                                Title = Strings.AddToKBText,
                                                Value = Strings.AddActionMessage,
                                            },
                                        },
                            },
                        },
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Title = Strings.SubmitResponseButtonText,
                                Data = new UpdateTicketResponsePayload
                                {
                                    TicketId = this.Ticket.TicketId,
                                    Action = UpdateTicketResponsePayload.AddRespondAction,
                                },
                            },
                        },
                    },
                });
            }
            else
            {
                actionsList.Add(new AdaptiveShowCardAction
                {
                    Title = Strings.RespondButtonText,
                    Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>
                        {
                             new AdaptiveTextBlock
                            {
                                Text = Strings.SMETicketCardYourAnswerLabel,
                                Wrap = true,
                                Size = AdaptiveTextSize.Small,
                            },
                             new AdaptiveTextInput
                            {
                                Spacing = AdaptiveSpacing.Small,
                                Id = nameof(UpdateTicketResponsePayload.AnswerForRespond),
                                Placeholder = Strings.ResponsePlaceholderText,
                                IsMultiline = true,
                                Value = this.updateTicketResponsePayload.AnswerForRespond,
                            },
                             new AdaptiveChoiceSetInput
                            {
                                Id = nameof(UpdateTicketResponsePayload.AddorAppendAction),
                                IsMultiSelect = true,
                                Style = AdaptiveChoiceInputStyle.Expanded,
                                Choices = new List<AdaptiveChoice>
                                        {
                                            new AdaptiveChoice
                                            {
                                                Title = Strings.AppendToKBText,
                                                Value = Strings.AppendActionMessage,
                                            },
                                        },
                            },
                        },
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Title = Strings.SubmitResponseButtonText,
                                Data = new UpdateTicketResponsePayload
                                {
                                    TicketId = this.Ticket.TicketId,
                                    Action = UpdateTicketResponsePayload.RespondAction,
                                },
                            },
                        },
                    },
                });
            }

            if (!string.IsNullOrEmpty(this.Ticket.KnowledgeBaseAnswer))
            {
                actionsList.Add(new AdaptiveShowCardAction
                {
                    Title = Strings.UpdateExistingButtonText,
                    Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = Strings.UpdateExistingButtonText,
                                Wrap = true,
                                Size = AdaptiveTextSize.Small,
                            },
                            new AdaptiveTextInput
                            {
                                Spacing = AdaptiveSpacing.Small,
                                Id = nameof(UpdateTicketResponsePayload.Answer),
                                Placeholder = Strings.ResponsePlaceholderText,
                                IsMultiline = true,
                                Value = this.updateTicketResponsePayload.Answer,
                            },
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveImage
                                            {
                                                Url = new Uri(string.Format("{0}/content/RedInfoIcon.png", appBaseUri)),
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                                AltText = "Info icon",
                                            },
                                        },
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = "auto",
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = Strings.WarningMessageOnUpdateAnswer,
                                                Wrap = true,
                                                Size = AdaptiveTextSize.Small,
                                                Spacing = AdaptiveSpacing.None,
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        Actions = new List<AdaptiveAction>
                        {
                            new AdaptiveSubmitAction
                            {
                                Title = Strings.SubmitResponseButtonText,
                                Data = new UpdateTicketResponsePayload
                                {
                                    TicketId = this.Ticket.TicketId,
                                    Action = UpdateTicketResponsePayload.UpdateResponseAction,
                                },
                            },
                        },
                    },
                });
            }

            return actionsList;
        }

        /// <summary>
        /// Create an adaptive card action that starts a chat with the user.
        /// </summary>
        /// <returns>Adaptive card action for starting chat with user.</returns>
        protected AdaptiveAction CreateChatWithUserAction()
        {
            var messageToSend = string.Format(CultureInfo.InvariantCulture, Strings.SMEUserChatMessage, this.Ticket.Title);
            var encodedMessage = Uri.EscapeDataString(messageToSend);

            return new AdaptiveOpenUrlAction
            {
                Title = string.Format(CultureInfo.InvariantCulture, Strings.ChatTextButton, this.Ticket.RequesterGivenName),
                Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(this.Ticket.RequesterUserPrincipalName)}&message={encodedMessage}"),
            };
        }
    }
}