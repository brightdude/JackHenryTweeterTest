using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching;
using Microsoft.Extensions.Caching.Memory;
using JHATest.Model;


namespace JHATest
{

    public class ReportService
    {
        /// <summary>
            /// RE: That said, you should think about how you would persist data if that was a requirement.
            /// If the level of real-time analytics is prevelant, I would keep data in Redis distributed cache server,where the aggregations created at time of persistence.
            /// With lesser degree, data can  be stored in CosmoDB, but if reporting needs are complex and undetermined,
            /// nothing have yet superseeded relational database. 
        /// </summary>
        static MemoryCache Cache { get; set; } = new MemoryCache(new MemoryCacheOptions());
        static MemoryCacheEntryOptions Options { get; set; } = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMilliseconds(50000));

        /// <summary>
            /// This is the cache name, in real world at times unique name is preffered, but it would scope-creep the asdsiment
        /// </summary>
        const string STREAMOFTWEETS = "StreamOfTweets";
        public static void AddTweet(string tweet)
        {
            if (!string.IsNullOrWhiteSpace(tweet))
            {
                TweetDTO dto = new TweetDTO() { Text = tweet, Hashtags = parseHashtags(tweet) };
                var tweetRepo = getTweetRepo();
                if (tweetRepo != null)
                {
                    tweetRepo.Add(dto);
                    Cache.Set(STREAMOFTWEETS, tweetRepo, Options);
                }
            }
        }
        private static List<TweetDTO> getTweetRepo()
        {
            List<TweetDTO> _tweetCache;
            if (Cache.TryGetValue(STREAMOFTWEETS, out _tweetCache))
            {
                var tweetRepo = Cache.Get<List<TweetDTO>>(STREAMOFTWEETS);
                if (tweetRepo != null)
                {
                    return tweetRepo;
                }
                else { 
                    return null;
                }                
            }
            else
            {
                try {
                    Cache.Set(STREAMOFTWEETS, new List<TweetDTO>(), Options);
                    return Cache.Get<List<TweetDTO>>(STREAMOFTWEETS);
                }
                catch (Exception ex) 
                {
                    throw new Exception("GET OR INIT OF TWEET REPOSITORY FAILED");
                } 
                
            }
        }
        public static async Task<List<string>> GetTop10Hashetags( )
        {
            var rawHashes= getTweetRepo().Select(s=>s.Hashtags).ToList();
            if (rawHashes.Count == 0)
            {
                var retval = rawHashes.SelectMany(s => s).GroupBy(q => q).OrderByDescending(gp => gp.Count()).Take(10).Select(g => g.Key).ToList();
                return await Task.FromResult(retval);
            }
            else {
                return null;
            }
            
        }
        public static async Task<int> getTotalTweetCount()
        {
            var repo = getTweetRepo();
            return await Task.FromResult(repo.Count);
        }
        private static List<string> parseHashtags(string input)
        {
            var regex = new Regex(@"#\w+");
            try
            {
                var hashtags = regex.Matches(input).Select(s => s.Value).ToList();
                return hashtags;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            { 
                throw new Exception(ex.Message);
            }

        }
    }
}
