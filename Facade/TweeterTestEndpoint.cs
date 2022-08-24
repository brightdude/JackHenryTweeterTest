using System;
using System.Runtime;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching;
using Microsoft.Extensions.Caching.Memory;


namespace JHATest
{
    public static class TweeterTestEndpoint
    {
        static MemoryCache _cache { get; set; } = new MemoryCache(new MemoryCacheOptions());
        static ReportService _reportService { get; set; }
        static ILogger Log { get; set; }

        [FunctionName("TweeterTest-Initialize")]
        public static async Task<string> Initialize([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            Log = log;
            string instanceId = await starter.StartNewAsync("TweeterTest-StartAggregating", null);
            if (instanceId == null) {
                log.LogError("Aggregation have failed to initialized");
                return null;
            }
            return "Aggregation Initialized";
        }

        [FunctionName("TweeterTest-StartAggregating")]
        public static async Task< string> StartAggregating([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync<string>("TweeterTest-AggregationStarted", "start");
            return "Aggregation Started";
        }
        
        
        [FunctionName("TweeterTest-AggregationStarted")]
        public static async Task AggregationStarted([ActivityTrigger] string command, ILogger log)
        {
            /// Re: "If there are other interesting statistics you’d like to collect, that would be great."
            /// This list of fields and expations can be exteded enabling to get indepth analytics from tweeter.
            /// For instance, one can investigate demographics correlation between certain hashtags, but in the scope of this exercise and absence of specifications I will refrain from that
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("tweet.fields", "author_id");

            await foreach (var dataitem in TweetDataService.ReadTweetStream(Log, "sample/stream", nvc, RestSharp.Method.Get))
            {
                if (dataitem != null && dataitem.data != null && !String.IsNullOrEmpty(dataitem.data.text))
                {
                    ///re: The app should process tweets as concurrently as possible to take advantage of available computing resources.
                    ///While this can be by utilizing Azure Event Grid or even simpler by wrapping the bellow call in another [ActivityTrigger] or event buffering/paging strategy etc,  
                    ///but for the sake of this excersise it would be exceivley complex.
                    ReportService.AddTweet(dataitem.data.text);
                }
            }
        }
        [FunctionName("TweeterTest-GetTop10Hashetags")]
        public static async Task<List<string>> GetTop10Hashetags([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            ///re: It’s very important that when the application receives a tweet it does not block statistics reporting while performing tweet processing.
            /// In this approach MemoryCache acts a rudementary semiphore of sorts as access to it is thread safe to a degree. Of course this is not ideal but very resilent still
            return await ReportService.GetTop10Hashetags();            
        }
        
       
        [FunctionName("TweeterTest-GetFeedCount")]
        public static async Task<string> GetFeedCount([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            return await Task.FromResult( $"At {DateTime.Now}, Total Tweets processed: {ReportService.getTotalTweetCount().Result }" );
        }

    }
}