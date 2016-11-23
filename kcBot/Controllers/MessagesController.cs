using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using kcBot;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using kcBot.Models;

namespace kcBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                // await Conversation.SendAsync(activity, () => new LUIS());
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                MessageObject.RootObject rootObject = await LUIS.interpretMessage(activity.Text);

                Activity reply;
               
               if(rootObject.topScoringIntent.intent == "getStockPrice")
               {
                    if (rootObject.entities.Length > 0)
                    {
                        string tickerSymbol = rootObject.entities[0].entity;
                        reply = activity.CreateReply($"{tickerSymbol} stock");
                        reply.Recipient = activity.From;
                        reply.Type = "message";
                        reply.Attachments = new List<Attachment>();

                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: $"http://ichart.finance.yahoo.com/b?s={tickerSymbol}"));

                        List<CardAction> cardButtons = new List<CardAction>();
                        CardAction plButton = new CardAction()
                        {
                            Value = $"http://money.cnn.com/quote/quote.html?symb={tickerSymbol}",
                            Type = "openUrl",
                            Title = "More Info"
                        };
                        cardButtons.Add(plButton);
                        string stockInfo = await Sotck.getStockAsync(tickerSymbol);
                        HeroCard plCard = new HeroCard()
                        {
                            Title = stockInfo,
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);
                    }else
                    {
                        reply = activity.CreateReply("We were unable to identify the ticker symbol");
                    }
                   
                }else
                {
                    reply = activity.CreateReply($"I'm sorry your intent is not clear");
                }
                

                //------------------------------------------------------------------

            
                //------------------------------------------------------------------
await connector.Conversations.SendToConversationAsync(reply);
                // return our reply to the user
                //Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters!");
                //string returnString = await Sotck.getStockAsync(activity.Text);
                //Activity reply = activity.CreateReply(returnString);
                //await connector.Conversations.ReplyToActivityAsync(reply);
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