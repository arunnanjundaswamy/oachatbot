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
    public class ClaimStatusDialog : IDialog<string>
    {
        const string NoOtherClaims = "Sorry, I could not find any other claim submitted by you.";
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsyncCustom("Sure, let me check on this.");
            var orderDetail = GetClaimDetail(context);

            PromptDialog.Text(context, ConfirmOrder, orderDetail);
        }

        private async Task ConfirmOrder(IDialogContext context, IAwaitable<string> result)
        {
            var userPref = await result;

            if (userPref.ContainsYes())
            {
                await ShowTheCurrentStatus(context);
                context.Done("close");
            }
            else if (userPref.ContainsNo())
            {
                await context.PostAsyncCustom(NoOtherClaims);
                context.Done("close");
            }
            else
            {
                //await context.PostAsyncCustom("Sorry, I am unable to understand.");
                PromptDialog.Text(context, ConfirmOrder, "Sorry, I am unable to understand. Can you please say yes or no?");
                //context.Done("quit");
            }
        }

        private async Task ShowTheCurrentStatus(IDialogContext context)
        {
            string statusMsg = $"Your claim will be reimbursed by {DateTime.Now.AddDays(2).ToShortDateString()}. Thank you.";
            await context.PostAsyncCustom(statusMsg);
        }

        private string GetClaimDetail(IDialogContext context)
        {
            return $"Are you referring to claim #234 on {DateTime.Now.AddDays(-2).ToShortDateString()}?";
        }
    }
}