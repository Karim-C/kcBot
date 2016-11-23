using kcBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace kcBot
{
    //[LuisModel("40966f67-3104-4eb2-983e-27f5648bb0a8", "b36fc035caad4d26abbe170b9cc4a05e")]
    //[Serializable]
    //public class LUIS : LuisDialog<object>
    //{
    //    [LuisIntent("getStockPrice")]
    //    public async Task getStockPrice(IDialogContext context, LuisServiceResult result)
    //    {
    //        await context.PostAsync("hello world");
    //        context.Wait(MessageReceived);
    //    }

    //    [LuisIntent("")]
    //    public async Task None(IDialogContext context, LuisServiceResult result)
    //    {
    //        await context.PostAsync("Nada");
    //        context.Wait(MessageReceived);
    //    }
    //}

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