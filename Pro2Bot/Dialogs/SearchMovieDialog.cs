using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Pro2Bot.Dialogs
{
    public class SearchMovieDialog : ComponentDialog
    {
        public SearchMovieDialog() : base(nameof(SearchMovieDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
           {
                SearchMovieAsync,
                GetMovieAsync,
                GetTrendingAsync,
           }));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TrendingMovieDialog());


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SearchMovieAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("What movie title do you want to search?") };
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> GetMovieAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var answer = (string)stepContext.Result;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var baseAddress = new Uri("http://api.themoviedb.org/3/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

                // api_key can be requestred on TMDB website
                using (var response = await httpClient.GetAsync("search/movie?api_key=265b44f5f18cefae943f3f30ec973d54&query=" + answer))
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    var model = JsonConvert.DeserializeObject<RootObject>(responseData);

                    foreach (var result in model.results)
                    {
                        var reply = new Attachment () { ContentUrl = " http://image.tmdb.org/t/p/w500" + result.poster_path, ContentType = "image/jpg" };
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(result.title), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(reply), cancellationToken);
                    }
                }
            }
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("So, do you want to know the trending movies right now?"),
                Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }),
            }, cancellationToken);
        }


        private async Task<DialogTurnResult> GetTrendingAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var lowercase = ((FoundChoice)stepContext.Result).Value.ToLower();

            if (lowercase == "yes")
            {
                return await stepContext.BeginDialogAsync(nameof(TrendingMovieDialog), null, cancellationToken);
            }
            else if (lowercase == "no")
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Okay, Thankyou for using my service, have a nice day"), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        public class Result
        {
            public bool adult { get; set; }
            public string backdrop_path { get; set; }
            public int id { get; set; }
            public string original_title { get; set; }
            public string release_date { get; set; }
            public string poster_path { get; set; }
            public double popularity { get; set; }
            public string title { get; set; }
            public bool video { get; set; }
            public double vote_average { get; set; }
            public int vote_count { get; set; }
        }

        public class RootObject
        {
            public int page { get; set; }
            public List<Result> results { get; set; }
            public int total_pages { get; set; }
            public int total_results { get; set; }
            public string url { get; set; }
        }
    }
}
