using kcBot.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace kcBot
{
    // class not used anymore   
    public class LUIS
    {
        public static async Task<MessageObject.RootObject> interpretMessage(string message)
        {

            HttpClient client = new HttpClient();
            string escapedMessage = Uri.EscapeDataString(message);
            string x = await client.GetStringAsync(new Uri("https://api.projectoxford.ai/luis/v2.0/apps/40966f67-3104-4eb2-983e-27f5648bb0a8?subscription-key=b36fc035caad4d26abbe170b9cc4a05e&q=" + escapedMessage + "&verbose=true"));

            MessageObject.RootObject rootObject = JsonConvert.DeserializeObject<MessageObject.RootObject>(x);

            return rootObject;
        }
    }
}