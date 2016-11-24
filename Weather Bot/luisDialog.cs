﻿using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
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

            PromptDialog.Text(context, ResumeAfter, "Please enter your name", "Try again",3);
        }

        private async Task ResumeAfter(IDialogContext context, IAwaitable<string> answer)
        {
            string name = await answer;
            context.UserData.SetValue("UserName", name);

            var text = $"You have successfully loged in {name}";
            await context.PostAsync(text);
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



        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {


            await context.PostAsync($"I have no idea what you are talking about.");
            context.Wait(MessageReceived);
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







        [LuisIntent("transfer")]
        public async Task RemoveTeam(IDialogContext context, LuisResult result)
        {

            EntityRecommendation rec;
            if (result.TryFindEntity("TeamName", out rec))
            {
                string teamName = rec.Entity;
                if (true)
                {
                    PromptDialog.Confirm(context, RemoveTeamAsync, $"Are you sure you want to delete {teamName}?");
                }
                else
                {
                    await context.PostAsync($"The team { teamName } was not found.");
                    context.Wait(MessageReceived);
                }
            }
            else
            {
                await context.PostAsync("You do not have enough funds to make this transfer.");
                context.Wait(MessageReceived);
            }
        }

        private async Task RemoveTeamAsync(IDialogContext context, IAwaitable<bool> result)
        {
            if (await result)
            {

                await context.PostAsync($"has been removed.");
            }
            else
            {
                await context.PostAsync($"OK, we wont remove them.");
            }
            context.Wait(MessageReceived);
        }
    }
}