using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TrendingMovieDialog : ComponentDialog
    {
        public TrendingMovieDialog() : base(nameof(TrendingMovieDialog))
        {
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    TrendingMovieAsync,
                }));
        }

        private async Task<DialogTurnResult> TrendingMovieAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var baseAddress = new Uri("http://api.themoviedb.org/3/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

                // api_key can be requestred on TMDB website
                using (var response = await httpClient.GetAsync("movie/popular?api_key=265b44f5f18cefae943f3f30ec973d54&language=en-US&page=1"))
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    var model = JsonConvert.DeserializeObject<TrendingRootObject>(responseData);

                    foreach (var result in model.results)
                    {
                        var reply = new Attachment() { ContentUrl = " http://image.tmdb.org/t/p/w500" + result.poster_path, ContentType = "image/jpg" };
                        await stepContext.Context.SendActivityAsync(MessageFactory.Text(result.title), cancellationToken);
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(reply), cancellationToken);
                    }
                }
            }
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Here are trending movies!"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thankyou for using my service, have a nice day"), cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }

        public class TrendingResult
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

        public class TrendingRootObject
        {
            public int page { get; set; }
            public List<TrendingResult> results { get; set; }
            public int total_pages { get; set; }
            public int total_results { get; set; }
            public string url { get; set; }
        }
    }
}
