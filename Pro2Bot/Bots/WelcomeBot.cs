using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace Pro2Bot.Bots
{
    public class WelcomeBot<T> : EchoBot<T> where T : Dialog
    {
        public WelcomeBot(ConversationState conversationState, UserState userState, T dialog, ILogger<EchoBot<T>> logger) : base(conversationState, userState, dialog, logger)
        {

        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var reply = MessageFactory.Text("Welcome to Guru Bot. " +
                        "I will help you for searching the movies you want.");
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
            }
        }
    }
}
