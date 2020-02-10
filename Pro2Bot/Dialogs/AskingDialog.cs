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
    public class AskingDialog : ComponentDialog
    {

        public AskingDialog() : base(nameof(AskingDialog))
        {
            // Array ini menentukan urutan execute
            var waterfallSteps = new WaterfallStep[]
            {
                FirstStepAsync,
                SecondStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new GenreDialog());
            AddDialog(new SearchMovieDialog());

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> FirstStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("So, do you want to search movie by title or genre?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Title", "Genre" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SecondStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var lowercase = ((FoundChoice)stepContext.Result).Value.ToLower();

            if (lowercase == "title")
            {
                return await stepContext.BeginDialogAsync(nameof(SearchMovieDialog), null, cancellationToken);
            }
            else if(lowercase == "genre")
            {
                return await stepContext.BeginDialogAsync(nameof(GenreDialog), null, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }
       
    }
}
