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
    public class UserProfileDialog : ComponentDialog
    {
        private const string UserInfo = "value-userInfo";

        public UserProfileDialog() : base(nameof(UserProfileDialog))
        {
            // Array ini menentukan urutan execute
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                LikeStepAsync,
                ChoiceStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new AskingDialog());

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[UserInfo] = new UserProfile();
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("My name is Guru. I want to know you more. Please enter your name.") };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LikeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Name = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ChoicePrompt),new PromptOptions 
            { 
                Prompt = MessageFactory.Text($"Hi {stepContext.Result}, do you like to watch movies?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }),
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ChoiceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.Choice = ((FoundChoice)stepContext.Result).Value;
            var lowercase = userProfile.Choice.ToLower();

            if (lowercase == "yes")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Nice, that's the answer I'm looking for!"), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(AskingDialog), null, cancellationToken);
            }
            else if (lowercase == "no")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Okay, seems you dont like watching movies, Thankyou for using my service."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please pick yes or no"), cancellationToken);
                return await stepContext.NextAsync(MessageFactory.Text("Please pick yes or no"), cancellationToken);            
            }
        }
    }
}
