using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OAChatBot.Extensions;
using OAChatBot.Luis;
using OAChatBot.Models;
using OAChatBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace OAChatBot.Dialogs
{
    [Serializable]
    public class ReturnItemDialog : IDialog<string>
    {
        private const string UnableToIdentifyProduct = "Sorry, could not identify the product. Can you please provide me the product detail again?";
        private const string UnableToIdentifyProductMax = "Sorry, could not identify the product.";
        protected string Product;
        private int _maxAttempts = 0;

        public ReturnItemDialog(string product)
        {
            Product = product;
        }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsyncCustom("I am so sorry for this.");
            _maxAttempts = 0;

            if (Product != "")
            {
                var product = await GetProductDetails(context, Product);

                if (string.IsNullOrEmpty(product))
                    PromptDialog.Text(context, ProductDetailComplete, "Can you please provide me the product detail?");
                else
                    PromptDialog.Text(context, UserProductConfirm, $"I can see {product}. Is this the one?");
            }
            else
            {
                PromptDialog.Text(context, ProductDetailComplete, "Can you please provide me the product detail?", UnableToIdentifyProduct, attempts: 3);
            }
        }

        private async Task ProductDetailComplete(IDialogContext context, IAwaitable<string> result)
        {
            var enteredProduct = await result;

            _maxAttempts++;

            if (_maxAttempts > 3)
            {
                await context.PostAsyncCustom(UnableToIdentifyProductMax);
                context.Done("close");
                _maxAttempts = 0;
                return;
            }

            await context.PostAsyncCustom("Thank you.");

            var product = await GetProductDetails(context, enteredProduct);

            if (product != null)
                PromptDialog.Text(context, UserProductConfirm, $"I can see {product}. Is this the one?");
            else
            {
                if (_maxAttempts >= 1)
                    PromptDialog.Text(context, ProductDetailComplete, UnableToIdentifyProduct, attempts: 3);
                else
                    PromptDialog.Text(context, ProductDetailComplete, "Can you provide me the product detail please?", UnableToIdentifyProduct, attempts: 3);
            }
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<Message> result)
        {
            context.Wait(MessageReceived);
        }

        private async Task UserProductConfirm(IDialogContext context, IAwaitable<string> result)
        {
            _maxAttempts = 0;
            var userPref = await result;

            if (userPref.ContainsYes())
            {
                var refundOptions = new List<string> { "Refund", "Replacement" };
                PromptDialog.Choice(context, RefundOptionsComplete, refundOptions, "Shall we send you a replacement, or shall I process a refund?", promptStyle: PromptStyle.PerLine);
                //CustomPromptDialog.Choice(context, RefundOptionsComplete, refundOptions, "Shall we send you a replacement, or shall I process a refund?", "Couldn't understand. Can you please enter your option again?", promptStyle: PromptStyle.PerLine);
            }
            else
                PromptDialog.Text(context, ProductDetailComplete, "Can you please provide me the product detail again?");
        }

        private async Task RefundOptionsComplete(IDialogContext context, IAwaitable<string> result)
        {
            var selOption = await result;

            if (String.Compare(selOption, "Refund", StringComparison.OrdinalIgnoreCase) == 0)
                await context.PostAsyncCustom("Will be replaced within 3 working days.");
            else
                await context.PostAsyncCustom("Will be refunded within 3 working days.");

            //PromptDialog.Confirm(context, FurtherAssistanceComplete, "Is there anything else we can assit you with?", promptStyle: PromptStyle.None);
            context.Done("Close");
        }

        private async Task<string> GetProductDetails(IDialogContext context, string product)
        {
            var luisClient = LuisClient.Create();
            var luisResp = await luisClient.SendQuery(product);
            var defaultProd = $"{product} Rs.200.00, \n Ordered Dt: {DateTime.UtcNow.AddHours(-2).GetIst().ToString("yy-MM-dd HH:mm:ss")}";

            if (luisResp != null)
            {
                Entity luisEntity = null;

                if (luisResp.TryFindType("Product", out luisEntity))
                {
                    product = luisEntity.Value;
                }
                else
                    return null;
            }

            UserContext userCntx = null;
            context.UserData.TryGetValue<UserContext>("userContext", out userCntx);
            var ordritems = await ServiceHandler.GetOrderItemForUser(product, userCntx);

            if (ordritems == null) return defaultProd;

            var orderItem = ordritems.FirstOrDefault();

            return orderItem == null ? defaultProd : $"Item: {orderItem.ItemName} {orderItem.Quantity} Rs.{orderItem.UnitPrice}, \n Ordered Dt: {orderItem.CreatedDt.ToString("yy-MM-dd HH:mm:ss")}";
        }
    }
}