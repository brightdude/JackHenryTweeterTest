using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using JHATest.Model;

namespace JHATest
{

    public class AuthenticationService
    {
        public static TweeterJWT JWT { get; set; }
        public static TweeterJWT GetToken(ILogger log)
        {
            log.LogInformation("getToken");
            var client = new RestClient("https://api.twitter.com/oauth2/token?grant_type=client_credentials");

            string APIKey = Environment.GetEnvironmentVariable("APIKey");
            string APISecret = Environment.GetEnvironmentVariable("APISecret");

            client.Authenticator = new HttpBasicAuthenticator(APIKey, APISecret);
            var request = new RestRequest("https://api.twitter.com/oauth2/token?grant_type=client_credentials", Method.Post);
            try
            {
                JWT = JsonConvert.DeserializeObject<TweeterJWT>(client.Execute(request).Content);
                return JWT;
            }
            catch (Exception ex) {
                log.LogError("getToken fauled ");
                return null;
            }
            
        }
    }


}
