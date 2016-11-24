using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using kcBot.Models;
using System.Collections.Generic;
using kcBot.DataModels;
using Microsoft.Bot.Builder.Dialogs;

namespace kcBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private string userName;

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

                StateClient stateClient = activity.GetStateClient();
                BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

                try {
                    await Conversation.SendAsync(activity, () => new luisDialog());
                }
                catch(Exception ex)
                {
                    Activity infoReply = activity.CreateReply(ex.Message);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                }
                


                /*
                string userMessage = activity.Text;

                string endOutput = string.Empty;


                //===================================================================================
                MessageObject.RootObject rootObject = await LUIS.interpretMessage(activity.Text);

                Activity reply;

                if (rootObject.topScoringIntent.intent == "getStockChart")
                { 
                    if (rootObject.entities.Length > 0 || userData.GetProperty<string>("tickerSymbol") != null )
                    {
                        string tickerSymbol = (rootObject.entities.Length > 0) ? rootObject.entities[0].entity : userData.GetProperty<string>("tickerSymbol");

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
                        await connector.Conversations.SendToConversationAsync(reply);
                    }
                    else
                    {
                        //reply = activity.CreateReply("We were unable to identify the ticker symbol");
                        endOutput = "We were unable to identify the ticker symbol";
                    }

                }

                if (rootObject.topScoringIntent.intent == "getStockPrice") 
                {
                    
                    if (rootObject.entities.Length > 0) 
                    {
                        string tickerSymbol = rootObject.entities[0].entity;
                        string stockInfo = await Sotck.getStockAsync(tickerSymbol);
                        endOutput = stockInfo;

                        userData.SetProperty<string>("tickerSymbol", tickerSymbol);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                    else
                    {
                        endOutput = "We were unable to identify the ticker symbol";
                    }
                        
                }

                // if the user intent is to greet the bot the bot will reply with a greeting and instructions if it is the users first hello
                if (rootObject.topScoringIntent.intent == "greeting") {
                    if (userData.GetProperty<bool>("Welcomed")) 
                    {
                        endOutput = "Hi again. If you need help let us know";
                    }else
                    {
                        userData.SetProperty<bool>("Welcomed", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                        reply = activity.CreateReply($"Hello, my name is Cassey. I am a chatBot and a representive for the 123 Bank.");
                        reply.Recipient = activity.From;
                        reply.Type = "message";
                        reply.Attachments = new List<Attachment>();

                        List<CardImage> cardImages = new List<CardImage>();
                        cardImages.Add(new CardImage(url: "https://cdn.pixabay.com/photo/2014/12/21/23/57/money-576443_960_720.png"));

                        List<CardAction> cardButtons = new List<CardAction>();
                        CardAction plButton = new CardAction()
                        {
                            Value = "http://www.hasbro.com/en-us/brands/monopoly",
                            Type = "openUrl",
                            Title = "Bank website"
                        };
                        cardButtons.Add(plButton);

                        HeroCard plCard = new HeroCard()
                        {
                            Title = "123 Bank",
                            Text = "Our mission here at the 123 Bank is to make every customer feel like our only customer.\n\n I can help you with the following:\n whats the price of MSFT\n show me a chart of MSFT \n",
                            Images = cardImages,
                            Buttons = cardButtons
                        };

                        Attachment plAttachment = plCard.ToAttachment();
                        reply.Attachments.Add(plAttachment);
                        await connector.Conversations.SendToConversationAsync(reply);


                    }
                }
                if (rootObject.topScoringIntent.intent == "transfer")
                {
                    if (userData.GetProperty<string>("UserName") != null) 
                    {
                        IDialogContext context;
                        //PromptDialog.Confirm(context, AfterConfirming_TurnOffAlarm, "Are you sure?", promptStyle: PromptStyle.None);
                        new PromptDialog.PromptConfirm("hello", "ok", 2, PromptStyle.Auto);

                    }
                    else
                    {
                        endOutput = "Please login before trying to make a transfer. (e.g. login Name password)";
                    }
                }
                    //===================================================================================

                    // calculate something for us to return
                    //    if (userData.GetProperty<bool>("SentGreeting"))
                    //{
                    //    endOutput = "Hello again";
                    //}
                    //else
                    //{
                    //    userData.SetProperty<bool>("SentGreeting", true);
                    //    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    //}



                    bool isWeatherRequest = true;

                if (userMessage.ToLower().Contains("clear"))
                {
                    endOutput = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    isWeatherRequest = false;
                }

                if (userMessage.Length > 9)
                {
                    if (userMessage.ToLower().Substring(0, 8).Equals("set home"))
                    {
                        string homeCity = userMessage.Substring(9);
                        userData.SetProperty<string>("HomeCity", homeCity);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        endOutput = homeCity;
                        isWeatherRequest = false;
                    }
                }

                if (userMessage.ToLower().Equals("home"))
                {
                    string homecity = userData.GetProperty<string>("HomeCity");
                    if (homecity == null)
                    {
                        endOutput = "Home City not assigned";
                        isWeatherRequest = false;
                    }
                    else
                    {
                        activity.Text = homecity;
                    }
                }

                if (userMessage.ToLower().Equals("msa"))
                {
                    Activity replyToConversation = activity.CreateReply("MSA information");
                    replyToConversation.Recipient = activity.From;
                    replyToConversation.Type = "message";
                    replyToConversation.Attachments = new List<Attachment>();
                    List<CardImage> cardImages = new List<CardImage>();
                    cardImages.Add(new CardImage(url: "https://cdn2.iconfinder.com/data/icons/ios-7-style-metro-ui-icons/512/MetroUI_iCloud.png"));
                    List<CardAction> cardButtons = new List<CardAction>();
                    CardAction plButton = new CardAction()
                    {
                        Value = "http://msa.ms",
                        Type = "openUrl",
                        Title = "MSA Website"
                    };
                    cardButtons.Add(plButton);
                    ThumbnailCard plCard = new ThumbnailCard()
                    {
                        Title = "Visit MSA",
                        Subtitle = "The MSA Website is here",
                        Images = cardImages,
                        Buttons = cardButtons
                    };
                    Attachment plAttachment = plCard.ToAttachment();
                    replyToConversation.Attachments.Add(plAttachment);
                    await connector.Conversations.SendToConversationAsync(replyToConversation);

                    return Request.CreateResponse(HttpStatusCode.OK);

                }

                if (userMessage.ToLower().Equals("get timelines"))
                {
                    List<moodTable> timelines = await AzureManager.AzureManagerInstance.GetBankRecords();
                    endOutput = "";
                    foreach (moodTable t in timelines)
                    {
                        endOutput += "[" + t.Name + "] Happiness \n\n";
                    }
                    isWeatherRequest = false;

                }
                //------------------------------------------------------------------------------------------------------
               

                if (userMessage.Equals("getBalance"))
                {
                    List<moodTable> timelines = await AzureManager.AzureManagerInstance.GetBankRecords();
                    endOutput = "";
                    string userName = userData.GetProperty<string>("UserName");
                    foreach (moodTable t in timelines)

                    {
                        if (t.Name == userName) {
                            endOutput = "Your current balance is $" + t.Balance;
                        }
                        
                    }

                   // await AzureManager.AzureManagerInstance.updateBalance(userData.GetProperty<string>("UserName"), 100);


                }

                if (userMessage.Length > 9)
                {
                    if (userMessage.ToLower().Substring(0, 8).Equals("set user"))
                    {
                        string userName = userMessage.Substring(9);
                        userData.SetProperty<string>("UserName", userName);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        endOutput = userName;
                       
                    }
                }
                //----------------------------------------------------------------------------------------------------------

                if (userMessage.ToLower().Equals("new timeline"))
                {
                    moodTable timeline = new moodTable()
                    {
 
                        
                        Name = "Chad",
                        UpdateDate = DateTime.Now
                    };

                    await AzureManager.AzureManagerInstance.AddTimeline(timeline);

                    isWeatherRequest = false;

                    endOutput = "New timeline added [" + timeline.Name + "]";
                }

                   // return our reply to the user
                   if (endOutput.Length > 0) 
                   {
                    Activity infoReply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                    }             
                    */      

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