using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml.XPath;
using OAChatBot.Extensions;
using OAChatBot.Models;
using OAChatBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace OAChatBot.Forms
{
    [Serializable]
    public class CancelOrderForm
    {
        public UserContext UserContext { get; set; }
        public List<BbOrder> OrdersToCancel { get; set; }

        public decimal? OrderTotalPrice;

        [Optional]
        [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
        [Template(TemplateUsage.NoPreference, "None")]
        [Template(TemplateUsage.EnumSelectOne, "Please select an Order {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
        public string Order { get; set; }

        public IForm<CancelOrderForm> BuildForm()
        {
            return new FormBuilder<CancelOrderForm>()
                    .Field(new FieldReflector<CancelOrderForm>(nameof(Order))
                            .SetType(null)
                           .SetDefine((state, field) =>
                           {

                               if (OrdersToCancel == null) return Task.FromResult(true);

                               foreach (var order in OrdersToCancel)
                               {
                                   var shippedDate = order.ShippedDate?.ToString("MM dd yyyy HH:mm") ?? "";
                                   var orderName = string.Format($"OrderId: {order.OrderID} TotalPrice: Rs.{order.TotalPrice} Status: {order.Comments}");
                                   field
                                       .AddDescription(order.OrderID.ToString(), orderName)
                                       .AddTerms(order.OrderID.ToString(), orderName);
                               }

                               return Task.FromResult(true);
                           }))
                            .Confirm(async (state) =>
                            {
                                if(state.Order == null)
                                    return new PromptAttribute("Are you sure you do not want to cancel any order?");

                                return new PromptAttribute("Are you sure you want to cancel this order?");
                            })
                    .OnCompletionAsync(async (context, orderForm) =>
                    {
                        await SelectionComplete(context, orderForm);
                    })
                    .Build();
        }

        private async Task SelectionComplete(IDialogContext context, CancelOrderForm orderForm)
        {
            if (orderForm?.Order != null)
            {
                await ServiceHandler.DeleteOrder(orderForm.Order, UserContext);
                var order = OrdersToCancel.First(item => item.OrderID == int.Parse(orderForm.Order));
                await context.PostAsyncCustom($"Cancelled the order. Your money of Rs.{order.TotalPrice} will be refunded within 24 hrs.");
            }
        }
    }
}