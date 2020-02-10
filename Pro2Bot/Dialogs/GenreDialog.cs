using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

namespace Pro2Bot.Dialogs
{
    public class GenreDialog : ComponentDialog
    {
        public GenreDialog() : base(nameof(GenreDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
           {
                GenreChoiceAsync,
                OptionAsync,
           }));

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ActionDialog());
            AddDialog(new HorrorDialog());
            AddDialog(new RomanceDialog());
            AddDialog(new ComedyDialog());

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GenreChoiceAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("So, what movie genre do you like to watch?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Action", "Horror", "Comedy", "Romance" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> OptionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var lowercase = ((FoundChoice)stepContext.Result).Value.ToLower();

            if (lowercase == "action")
            {
                return await stepContext.BeginDialogAsync(nameof(ActionDialog), null, cancellationToken);
            }
            else if (lowercase == "horror")
            {
                return await stepContext.BeginDialogAsync(nameof(HorrorDialog), null, cancellationToken);
            }
            else if (lowercase == "comedy")
            {
                return await stepContext.BeginDialogAsync(nameof(ComedyDialog), null, cancellationToken);
            }
            else if (lowercase == "romance")
            {
                return await stepContext.BeginDialogAsync(nameof(RomanceDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
    }
}
