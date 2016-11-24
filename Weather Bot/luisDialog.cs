using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Threading.Tasks;

namespace kcBot
{
    [LuisModel("40966f67-3104-4eb2-983e-27f5648bb0a8", "b36fc035caad4d26abbe170b9cc4a05e")]
    //[LuisModel("40966f67-3104-4eb2-983e-627f5648bb0a8", "b36fc035c6aad4d26abbe170b9cc4a05e")]
    [Serializable]
    public class luisDialog : LuisDialog<object>
    {


        [LuisIntent("help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {

            string reply = "help";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }
        /*
        [LuisIntent("login")]
        public async Task login(IDialogContext context, LuisResult result)
        {
            string userName = "";
            if (!(context.UserData.TryGetValue("UserName", out userName)))
            {
                PromptDialog.Text(context, AfterUserInputUserName, "Please enter your name", "Try again message", 2);
            }
            context.Wait(MessageReceived);


        }

        private ResumeAfter<string> AfterUserInputUserName(IDialogContext context, IAwaitable<string> result)
        {

            context.UserData.SetValue("UserName", result);

        }
        */
        [LuisIntent("login")]
        public async Task login(IDialogContext context, LuisResult result)
        {

            PromptDialog.Text(context, ResumeAfterUserName, "Please enter your name", "Try again", 3);

        }

        private async Task ResumeAfterUserName(IDialogContext context, IAwaitable<string> answer)
        {
            string name = await answer;
            context.UserData.SetValue("UserName", name);

            var text = $"Name: {name}";
            await context.PostAsync(text);
            PromptDialog.Text(context, ResumeAfterPassword, "Please enter your password", "Try again", 3);
            // context.Wait(MessageReceived);
        }

        private async Task ResumeAfterPassword(IDialogContext context, IAwaitable<string> answer)
        {
            string password = await answer;
            string userName = "";
            context.UserData.TryGetValue("UserName", out userName);

            string passwordInDB = await AzureManager.AzureManagerInstance.getPassward(userName);

            string endOutput = "";

            if (password.Equals(passwordInDB))
            {
                context.UserData.SetValue("loggedIn", true);
                endOutput = $"You have successfully loged in {userName}";
            }
            else
            {
                endOutput = $"You were not successfully at login in";
            }


            await context.PostAsync(endOutput);
            context.Wait(MessageReceived);
        }


        [LuisIntent("getStockChart")]
        public async Task getStockChart(IDialogContext context, LuisResult result)
        {
            string tickerSymbol = "";

            if (result.Entities.Count > 0 || context.UserData.TryGetValue("tickerSymbol", out tickerSymbol))
            {


                tickerSymbol = (result.Entities.Count > 0) ? result.Entities[0].Entity : tickerSymbol;

                Microsoft.Bot.Connector.Activity reply = (Microsoft.Bot.Connector.Activity)context.MakeMessage();

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
                // await connector.Conversations.SendToConversationAsync(reply);
                await context.PostAsync(reply);
                context.Wait(MessageReceived);
            }
            else
            {
                //reply = activity.CreateReply("We were unable to identify the ticker symbol");
                await context.PostAsync("We were unable to identify the ticker symbol");
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("getBalance")]
        public async Task getBalance(IDialogContext context, LuisResult result)
        {
            string endOutput = string.Empty;
            string userName = string.Empty;
            if (context.UserData.TryGetValue("UserName", out userName))
            {
               double balance = await AzureManager.AzureManagerInstance.getBalance(userName);
                endOutput = "$" + balance.ToString();
            }
            else
            {
                endOutput = "Please login before trying to check your bank balance.";
            }
            await context.PostAsync(endOutput);
            context.Wait(MessageReceived);
        }



        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"Your intent is not clear. Please rephrase the sentence.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("getStockPrice")]
        public async Task getStockPrice(IDialogContext context, LuisResult result)
        {
            string endOutput = string.Empty;
            if (result.Entities.Count > 0)
            {
                string tickerSymbol = result.Entities[0].Entity;
                string stockInfo = await Sotck.getStockAsync(tickerSymbol);
                endOutput = stockInfo;

                context.UserData.SetValue<string>("tickerSymbol", tickerSymbol);

            }
            else
            {
                endOutput = "We were unable to identify the ticker symbol";
            }
            await context.PostAsync(endOutput);
            context.Wait(MessageReceived);
        }


        [LuisIntent("greeting")]
        public async Task Greet(IDialogContext context, LuisResult result)
        {
            bool Welcomed = false;

            if (context.UserData.TryGetValue("Welcomed", out Welcomed))
            {

                await context.PostAsync("Hi again. If you need help let us know");
                context.Wait(MessageReceived);
            }
            else
            {

                context.UserData.SetValue<bool>("Welcome", true);

                Microsoft.Bot.Connector.Activity reply = (Microsoft.Bot.Connector.Activity)context.MakeMessage();

                reply.Type = "message";

                reply.Attachments = new List<Attachment>();

                await context.PostAsync("Hello, my name is Cassey. I am a chatBot and a representive for the Contoso Bank.\n I can help you with stocks and transfers.");
                // await context.PostAsync("Hello, my name is Cassey. I am a chatBot and a representive for the 123 Bank.\n I can help you with the following:\n stocks (e.g. what's the price of MSFT or show me a chart of MSFT)\n If you login I can also help you with transfers.\n");
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
                    Title = "Contoso Bank",
                    Text = "Our mission here at the Contoso Bank is to make every customer feel like our only customer.",
                    Images = cardImages,
                    Buttons = cardButtons
                };

                Attachment plAttachment = plCard.ToAttachment();
                reply.Attachments.Add(plAttachment);
                await context.PostAsync(reply);
                context.Wait(MessageReceived);
            }
        }



        private string reciever;
        private double transferAmount;
        [LuisIntent("transfer")]
        public async Task transfer(IDialogContext context, LuisResult result)
        {
            string endOutput = "";
            string userName = "";
            if (context.UserData.TryGetValue("UserName", out userName))
            {
                transferAmount = 0;
                string strAmount = result.Entities.FirstOrDefault(e => e.Type == "number").Entity;
                reciever = result.Entities.FirstOrDefault(e => e.Type == "reciever").Entity;

                if (result.Entities.Count == 2 && double.TryParse(strAmount, out transferAmount))
                {
                    double currentBalance = await AzureManager.AzureManagerInstance.getBalance(userName);
                    if (currentBalance > transferAmount)
                    {

                        PromptDialog.Confirm(context, AfterConfirmingTransfer, $"Do you want to confirm the transfer of ${transferAmount} to {reciever}?", promptStyle: PromptStyle.None);

                        
                    }
                    else
                    {
                        endOutput = "You do not have sufficient funds";
                        await context.PostAsync(endOutput);
                        context.Wait(MessageReceived);
                        
                    }
                }
                else
                {
                    endOutput = "Please try again (e.g. I want to transfer $100 dollars to Bob)";
                    await context.PostAsync(endOutput);
                    context.Wait(MessageReceived);

                }


            }
            else
            {
                endOutput = "Please login before trying to make a transfer";
                await context.PostAsync(endOutput);
                context.Wait(MessageReceived);

            }

        }



        public async Task AfterConfirmingTransfer(IDialogContext context, IAwaitable<bool> confirmation)
        {
            string userName = "";
            if (await confirmation && context.UserData.TryGetValue("UserName", out userName))
            {
                
                await AzureManager.AzureManagerInstance.updateBalance(reciever, transferAmount);
                transferAmount = transferAmount * -1;
                await AzureManager.AzureManagerInstance.updateBalance(userName, transferAmount);

                transferAmount = transferAmount * -1;
                String endOutput = $"The transfer of ${transferAmount} to {reciever} was successful.";
                await context.PostAsync(endOutput);
            }
            else
            {
                await context.PostAsync("The transfer was canceled.");
            }

            context.Wait(MessageReceived);
           
        }

    }   
}