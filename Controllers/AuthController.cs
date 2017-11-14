using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace EduBot.Controllers
{
    public class AuthController : ApiController
    {
        public AuthController()
        {
        }

        private static readonly HttpClient client = new HttpClient();


        [Route("api/auth/admin")]
        public string GetAdmin()
        {

            IEnumerable<KeyValuePair<string, string>> queryString = Request.GetQueryNameValuePairs();
            string admin_consent = queryString.Where(nv => nv.Key == "admin_consent").Select(nv => nv.Value).FirstOrDefault();

            return admin_consent;
        }



        [Route("api/auth")]
        public string Get()
        {

            //foreach (var parameter in Request.GetQueryNameValuePairs())
            //{
            //    var key = parameter.Key;
            //    var value = parameter.Value;
            //}
            IEnumerable<KeyValuePair<string, string>> queryString = Request.GetQueryNameValuePairs();
            string stateString = queryString.Where(nv => nv.Key == "state").Select(nv => nv.Value).FirstOrDefault();
            string codeString = queryString.Where(nv => nv.Key == "code").Select(nv => nv.Value).FirstOrDefault();
            //string admin_consent = queryString.Where(nv => nv.Key == "admin_consent").Select(nv => nv.Value).FirstOrDefault();

            //if (admin_consent == "True")
            //{
            //    PostForTokenAsync();
            //}
            var stateObj = JsonConvert.DeserializeObject<State>(stateString);

            PostForTokenAsync(stateObj, codeString);

            //return Request.GetQueryNameValuePairs();
            return "Success";
        }


        private async void PostForTokenAsync(State state, string code)
        {
            var values = new Dictionary<string, string>
            {
                { "client_id", "255ef9af-d650-4a4d-822d-bbf606211dfa" },
                //{ "scope", "offline_access%20eduroster.readwrite%20eduassignments.readwrite" },
                { "scope", "mail.read" },
                { "code", code },
                { "redirect_uri", "http://localhost:8080/api/auth/" },
                { "client_secret", "kercqXHZJ0037_+ixLLO9=*" },
                { "grant_type", "authorization_code" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync("https://login.microsoftonline.com/common/oauth2/v2.0/token", content);

            var responseData = await response.Content.ReadAsAsync<TokenResponse>();

            var botCred = new MicrosoftAppCredentials(
                ConfigurationManager.AppSettings["MicrosoftAppId"],
                ConfigurationManager.AppSettings["MicrosoftAppPassword"]);
            var stateClient = new StateClient(botCred);
            BotState botState = new BotState(stateClient);
            BotData botData = new BotData(eTag: "*");
            botData.SetProperty<string>("AccessToken", responseData.access_token);
            botData.SetProperty<string>("RefreshToken", responseData.refresh_token);
            await stateClient.BotState.SetUserDataAsync(state.channelId, state.userId, botData);
        }
    }

    public class TokenResponse
    {

        [JsonProperty("token_type")]
        public string token_type { get; set; }

        [JsonProperty("scope")]
        public string scope { get; set; }

        [JsonProperty("expires_in")]
        public int expires_in { get; set; }

        [JsonProperty("access_token")]
        public string access_token { get; set; }

        [JsonProperty("refresh_token")]
        public string refresh_token { get; set; }
    }

    public class State
    {
        public string channelId { get; set; }
        public string userId { get; set; }
    }
}