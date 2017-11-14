using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace EduBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        public class State
        {
            public string channelId { get; set; }
            public string userId { get; set; }
        }
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    if (activity.Text == "login")
                    {
                        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                        Activity replyToConversation = activity.CreateReply();
                        replyToConversation.Recipient = activity.From;
                        replyToConversation.Type = "message";

                        State state = new State()
                        {
                            channelId = activity.ChannelId,
                            userId = activity.From.Id
                        };

                        replyToConversation.Attachments = new List<Attachment>();
                        List<CardAction> cardButtons = new List<CardAction>();
                        CardAction plButton = new CardAction()
                        {
                            //Value = $"{System.Configuration.ConfigurationManager.AppSettings["AppWebSite"]}/Home/Login?userid={HttpUtility.UrlEncode(activity.From.Id)}",
                            //Value = "https://login.microsoftonline.com/common/adminconsent?client_id=255ef9af-d650-4a4d-822d-bbf606211dfa&state=auth&redirect_uri=http://localhost:8080/api/auth/",
                            //Value = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=255ef9af-d650-4a4d-822d-bbf606211dfa&response_type=code&redirect_uri=http://localhost:8080/api/auth/&response_mode=query&scope=offline_access%20eduroster.readwrite%20eduassignments.readwrite&state={state}",
                            Value = $"https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=255ef9af-d650-4a4d-822d-bbf606211dfa&response_type=code&redirect_uri=http://localhost:8080/api/auth/&response_mode=query&scope=mail.read&state={HttpUtility.UrlEncode(JsonConvert.SerializeObject(state))}",
                            Type = "signin",
                            Title = "Authentication Required"
                        };
                        cardButtons.Add(plButton);
                        SigninCard plCard = new SigninCard("Please login to Office 365", new List<CardAction>() { plButton });
                        Attachment plAttachment = plCard.ToAttachment();
                        replyToConversation.Attachments.Add(plAttachment);

                        var reply = await connector.Conversations.SendToConversationAsync(replyToConversation);
                    }
                    else if (activity.Text == "code")
                    {

                        // Get access token from bot state
                        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        StateClient stateClient = activity.GetStateClient();
                        BotState botState = new BotState(stateClient);
                        BotData botData = await botState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                        string token = botData.GetProperty<string>("AccessToken");
                        string refresh = botData.GetProperty<string>("RefreshToken");

                        Activity reply = activity.CreateReply($"token is {token} & refresh is {refresh}");
                        await connector.Conversations.ReplyToActivityAsync(reply);
                    }
                }
                //await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}