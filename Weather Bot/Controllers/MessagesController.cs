using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

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


                var attachedImage = activity.Attachments?.FirstOrDefault(a => a.ContentType.Contains("image"));
                if (attachedImage != null)
                {
                   
                    VisionServiceClient VisionServiceClient = new VisionServiceClient("9a4403a0455e4d59840bd0095c45c324");
                    Activity reply;
                    try
                    {
                        AnalysisResult analysisResult = await VisionServiceClient.DescribeAsync(activity.Attachments[0].ContentUrl, 3);
                        reply = activity.CreateReply($"Your financial goal is to own/experience: {analysisResult.Description.Captions[0].Text}");
                        // AzureManager.AzureManagerInstance.updateGoal($"Your financial goal is to own/experience: {analysisResult.Description.Captions[0].Text}");
                    }
                    catch (Exception ex)
                    {
                        reply = activity.CreateReply("An error occured sending your image, try again later.");

                    } 
                                       
                    await connector.Conversations.ReplyToActivityAsync(reply);
                    

                } else
                {
                    try {
                        await Conversation.SendAsync(activity, () => new luisDialog());
                    }
                    catch (Exception ex)
                    {
                        Activity infoReply = activity.CreateReply($"There was an error (oops):{ ex.Message}");
                        await connector.Conversations.ReplyToActivityAsync(infoReply);
                    }
                }
                
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