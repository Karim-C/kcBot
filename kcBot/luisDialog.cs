using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;


namespace kcBot
{
    [LuisModel("40966f67-3104-4eb2-983e-27f5648bb0a8", "b36fc035caad4d26abbe170b9cc4a05e")]
    [Serializable]
    public class luisDialog : LuisDialog<object>
    {


        // This method lets the user know what the bot can do if they ask for help
        [LuisIntent("help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {

            string reply = "You can ask for the price of stocks and to see the charts. You can check your bank balance and transfer money to someone else. You can also create an account or delete your current account.";
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        // This method allows the user to login
        [LuisIntent("login")]
        public async Task login(IDialogContext context, LuisResult result)
        {

            PromptDialog.Text(context, ResumeAfterUserName, "Please enter your name", "Try again", 3);

        }

        // Starts after the user has entered their name
        private async Task ResumeAfterUserName(IDialogContext context, IAwaitable<string> answer)
        {
            string name = await answer;
            context.UserData.SetValue("UserName", name);

            var text = $"Name: {name}";
            await context.PostAsync(text);

            // Prompts the user to enter their password
            PromptDialog.Text(context, ResumeAfterPassword, "Please enter your password", "Try again", 3);

        }

        // Starts when the user has entered their password and checks the login information is correct
        private async Task ResumeAfterPassword(IDialogContext context, IAwaitable<string> answer)
        {
            string password = await answer;
            string userName = "";
            context.UserData.TryGetValue("UserName", out userName);
            string endOutput = "";

            try
            {
                string passwordInDB = await AzureManager.AzureManagerInstance.getPassword(userName);

                // Checks whether the given password matches that in the database
                if (password.Equals(passwordInDB))
                {
                    context.UserData.SetValue("loggedIn", true);
                    endOutput = $"You have successfully logged in {userName}";
                }
                else
                {
                    endOutput = $"You were not successfully at logging in, try again";
                }
            }
            catch (Exception ex)
            {
                endOutput = $"This name does not exist in the database.";
            }

            await context.PostAsync(endOutput);
            context.Wait(MessageReceived);
        }

        // Displays a stock chart when asked by the user
        [LuisIntent("getStockChart")]
        public async Task getStockChart(IDialogContext context, LuisResult result)
        {
            string tickerSymbol = "";

            // Ensures a ticker symbol has been specified
            if (result.Entities.Count > 0 || context.UserData.TryGetValue("tickerSymbol", out tickerSymbol))
            {
                // First checks whether ticker symbol has been specified otherwise takes most recently specified ticker symbol
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
                await context.PostAsync(reply);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("We were unable to identify the ticker symbol");
                context.Wait(MessageReceived);
            }
        }

        // Retrieves the users bank balance form the database and displays it to them
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


        // When the users intent is unclear
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            await context.PostAsync($"Your intent is not clear. Please rephrase the sentence.");
            context.Wait(MessageReceived);
        }

        // Retrieves the price of a stock
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

        // Greets the user. The first time the user greets the bot a large welcome message is displayed
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

                context.UserData.SetValue("Welcomed", true);

                Microsoft.Bot.Connector.Activity reply = (Microsoft.Bot.Connector.Activity)context.MakeMessage();

                reply.Type = "message";

                reply.Attachments = new List<Attachment>();

                await context.PostAsync("Hello, my name is Cassey. I am a chatBot and a representative for the Contoso Bank.\n I can help you with stocks and transfers.");
                reply.Type = "message";
                reply.Attachments = new List<Attachment>();

                List<CardImage> cardImages = new List<CardImage>();
                cardImages.Add(new CardImage(url: "https://cdn.pixabay.com/photo/2013/07/12/14/07/bag-147782_640.png"));

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


        // Allows for the transfer of money between 2 people
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

                // Checks that a receiver and an amount has been identified in the users message
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


        // Once the transfer has been confirmed this method starts and alters the records in the database
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

        // Deletes the account of the user
        [LuisIntent("delete")]
        public async Task Delete(IDialogContext context, LuisResult result)
        {
            bool loggedIn = false;
            string userName = "";

            // Checks whether the user is logged in
            if (context.UserData.TryGetValue("loggedIn", out loggedIn))
            {
                if (context.UserData.TryGetValue("loggedIn", out userName))
                {
                    PromptDialog.Confirm(context, AfterConfirmingDelete, $"Please confirm that you want to delete your account (This is an irreversible action)", promptStyle: PromptStyle.None);

                }
            }
            else
            {
                string reply = "Please login before trying to delete your account";
                await context.PostAsync(reply);
                context.Wait(MessageReceived);
            }


        }



        public async Task AfterConfirmingDelete(IDialogContext context, IAwaitable<bool> confirmation)
        {
            string userName = "";
            // Checks whether the user has confirmed the transfer and whether the users name has been stored
            if (await confirmation && context.UserData.TryGetValue("UserName", out userName))
            {

                await AzureManager.AzureManagerInstance.DeleteBankRecord(userName);
                String endOutput = $"Deletion was successful.";

                // Logs the user out by clearing user information
                context.UserData.RemoveValue("UserName");
                context.UserData.RemoveValue("LoggenIn");

                await context.PostAsync(endOutput);
            }
            else
            {
                await context.PostAsync("Deletion was canceled.");
            }

            context.Wait(MessageReceived);

        }

        // Creates a record in the database
        [LuisIntent("create")]
        public async Task Create(IDialogContext context, LuisResult result)
        {

            PromptDialog.Text(context, ResumeAfterUserNameCreate, "Please enter your name", "Try again", 3);

        }

        // Starts when the user has entered their name and stores the information
        private async Task ResumeAfterUserNameCreate(IDialogContext context, IAwaitable<string> answer)
        {
            string name = await answer;
            context.UserData.SetValue("UserName", name);

            var text = $"Name: {name}";
            await context.PostAsync(text);

            PromptDialog.Text(context, ResumeAfterPasswordCreate, "Please enter your password", "Try again", 3);

        }

        // Starts when the user has entered their password and stores the information in the database
        private async Task ResumeAfterPasswordCreate(IDialogContext context, IAwaitable<string> answer)
        {
            string password = await answer;
            string userName = "";
            string endOutput = "";
            context.UserData.TryGetValue("UserName", out userName);

            try
            {
                await AzureManager.AzureManagerInstance.AddBankRecord(userName, password);

                context.UserData.SetValue("loggedIn", true);
                endOutput = $"{userName}, you have successfully created an account with us here at Contoso Bank, congratulations.";
            }
            catch (Exception ex)
            {
                string errMsg = ex.Message;
                endOutput = $"There was an error: {errMsg}, try again.";
            }

            await context.PostAsync(endOutput);
            context.Wait(MessageReceived);
        }

    }
}