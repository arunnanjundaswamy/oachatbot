using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using OAChatBot.Extensions;
using OAChatBot.Models;
using OAChatBot.Services;
using Microsoft.Bot.Builder.Dialogs;

namespace OAChatBot.Dialogs
{
    [Serializable]
    public class DeliveryStatusDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsyncCustom("Sure, let me check on this.");
            var orderDetail = await GetOrderDetail(context);

            PromptDialog.Text(context, ConfirmOrder, orderDetail);
        }

        private async Task ConfirmOrder(IDialogContext context, IAwaitable<string> result)
        {
            var userPref = await result;

            if (userPref.ContainsYes())
            {
                await ShowTheCurrentStatus(context);
                PromptDialog.Text(context, ConfirmNextDel, "What time you perfer today?");
            }
            else if (userPref.ContainsNo())
            {
                await context.PostAsyncCustom("Sorry, I could not find any other order.");
                context.Done("close");
            }
            else
            {
                //await context.PostAsyncCustom("Sorry, I am unable to understand.");
                PromptDialog.Text(context, ConfirmOrder, "Sorry, I am unable to understand. Can you please say yes or no?");
                //context.Done("quit");
            }
        }

        private async Task ConfirmNextDel(IDialogContext context, IAwaitable<string> result)
        {
            string statusMsg = "We will inform the Logistics team, they will give you a call.";
            await context.PostAsyncCustom(statusMsg);
            context.Done("close");
        }

        private async Task ShowTheCurrentStatus(IDialogContext context)
        {
            string statusMsg = "Our team had come to your door step at 9:15 AM. Your door bell was not answered hence our team came back.";
            await context.PostAsyncCustom(statusMsg);
        }

        private async Task<string> GetOrderDetail(IDialogContext context)
        {
            UserContext userCntx = null;
            context.UserData.TryGetValue<UserContext>("userContext", out userCntx);

            var orders = await ServiceHandler.GetLatestOrdersForUser(userCntx);

            if (orders == null || orders.Count == 0)
                return "Are you referring to order # 1211 for Rs.2348?";

            var order = orders.First();
            return $"Are you referring to order # {order.OrderID} for Rs.{order.TotalPrice}?";
        }
    }
}