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
    public class ReturnOrderForm
    {
        public UserContext UserContext { get; set; }
        public List<BbOrder> RecentOrders { get; set; }
        public List<BbOrderItem> OrderItems { get; set; }


        public BbOrderItem SelectedOrderItem;
        public decimal? OrderTotalPrice;


        [Optional]
        [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
        [Template(TemplateUsage.NoPreference, "None")]
        [Template(TemplateUsage.EnumSelectOne, "Please select an Order {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
        public string Order { get; set; }

        [Optional]
        [Template(TemplateUsage.NotUnderstood, "I do not understand \"{0}\".", "Try again, I don't get \"{0}\".")]
        [Template(TemplateUsage.NoPreference, "None")]
        [Template(TemplateUsage.EnumSelectOne, "Please select an Order Item {||}", ChoiceStyle = ChoiceStyleOptions.PerLine)]
        public string OrderItem { get; set; }

        public IForm<ReturnOrderForm> BuildForm()
        {
            return new FormBuilder<ReturnOrderForm>()
                    .Field(new FieldReflector<ReturnOrderForm>(nameof(Order))
                            .SetType(null)
                           .SetDefine((state, field) =>
                           {

                               if (RecentOrders == null) return Task.FromResult(true);

                               foreach (var order in RecentOrders)
                               {
                                   var shippedDate = order.ShippedDate?.ToString("MM dd yyyy HH:mm") ?? "";
                                   var orderName = string.Format($"OrderId: {order.OrderID} TotalPrice: Rs.{order.TotalPrice} Delivered on: {shippedDate}");
                                   field
                                       .AddDescription(order.OrderID.ToString(), orderName)
                                       .AddTerms(order.OrderID.ToString(), orderName);
                               }

                               return Task.FromResult(true);
                           }))
                           .Field(new FieldReflector<ReturnOrderForm>(nameof(OrderItem))
                            .SetType(null)
                           .SetDefine(async (state, field) =>
                           {
                               if (state.Order != null)
                               {
                                   if (String.Compare(state.Order, "none", StringComparison.OrdinalIgnoreCase) == 0) return await Task.FromResult(true);

                                   this.OrderItems = await ServiceHandler.GetOrderItemsForOrder(int.Parse(state.Order));
                                   foreach (var orderItem in this.OrderItems)
                                   {
                                       var itemName = string.Format($"{orderItem.ItemName} Qty:{orderItem.Quantity} Rs.{orderItem.UnitPrice} ");
                                       field
                                           .AddDescription(orderItem.OrderItemID.ToString(), itemName)
                                           .AddTerms(orderItem.OrderItemID.ToString(), itemName);
                                   }
                               }
                               return await Task.FromResult(true);
                           }))

                    .OnCompletionAsync(async (context, orderForm) =>
                    {
                        await SelectionComplete(context, orderForm);
                    })
                    .Build();
        }

        private async Task SelectionComplete(IDialogContext context, ReturnOrderForm orderForm)
        {
            if (orderForm?.OrderItem != null)
            {
                await ServiceHandler.DeleteOrderItem(int.Parse(orderForm.OrderItem));
                var itemPrice = OrderItems.First(item => item.OrderItemID == int.Parse(orderForm.OrderItem));
                await context.PostAsyncCustom($"Deleted the order item. Your money of Rs.{itemPrice.UnitPrice} will be refunded.");
            }
        }
    }
}