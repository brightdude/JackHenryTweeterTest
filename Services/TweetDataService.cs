using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.Json;
using RestSharp.Authenticators;
using RestSharp.Extensions;



namespace JHATest
{
    public class TwitterSingleObject
    {
        public Data data { get; set; }
    }
    public class Data
    {
        public string author_id { get; set; }
        public string id { get; set; }
        public string text { get; set; }
    }
    public class TweetDataService
    {
        public static async IAsyncEnumerable<TwitterSingleObject> ReadTweetStream(ILogger log, string methodName, NameValueCollection nvc, Method methodType)
        {
            string result = string.Empty;
            string baseurl = @"https://api.twitter.com/2/tweets";



            var options = new RestClientOptions("https://api.twitter.com/2");

            var token = AuthenticationService.GetToken(log).access_token;
            var client = new RestClient(options);
            client.Authenticator = new RestSharp.Authenticators.OAuth2.OAuth2AuthorizationRequestHeaderAuthenticator(token, "Bearer");            
            CancellationToken cancellationToken = default;
            string url = @$"{baseurl}/{methodName}{ToQueryString(nvc)}";
            
            //var url = @"https://api.twitter.com/2/tweets/sample/stream?tweet.fields=author_id";

            var response = client.StreamJsonAsync<TwitterSingleObject>(url, cancellationToken);

            await foreach (var item in response.WithCancellation(cancellationToken))
            {
                yield return item;
            }
        }

        private static string ToQueryString(NameValueCollection nvc)
        {
            var array = (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select string.Format("{0}={1}",HttpUtility.UrlEncode(key),HttpUtility.UrlEncode(value))).ToArray();
            return "?" + string.Join("&", array);
        }
    }
}
