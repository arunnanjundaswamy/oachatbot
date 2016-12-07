using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using OAChatBot.Services;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using OAChatBot.Models;
using System.Linq;
using System.Text;
using OAChatBot.Extensions;
using OAChatBot.Forms;
using OAChatBot.Luis;
using Microsoft.Bot.Builder.FormFlow;
using OAChatBot.Repository;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Resource;

namespace OAChatBot.Dialogs
{
    [LuisModel("04f4e0f6-7ffc-4d9c-b967-9d148cd63246", "48787abcea414d8da37294e4ccba8997")]
    [Serializable]
    public class OAConversationLuisDialog : LuisDialog<string>
    {
        private const string GuideMsg = "* Order items from BB \n E.g.: Can I get 5 kgs of Ashirvad atta? \n " +
                                        "* Repeat the previous ordered item. E.g. type **add again** or can you please add it again \n " +
                                        "* Know the status of your order. E.g.: May I know the status of my order?. \n " +
                                        "* Return an ordered item . E.g. Would like to return back 2 kgs of atta I have purchased. \n " +
                                        "* Type **offers** to know the available offers. \n " +
                                         "* Type **quit or exit** to exit from chat.";
        private const string OperationsErrorMsg = "Error in performing the operation. Please try after some time.";
        private const string OrderSuccessMsg =
            "Generated a new order with \n Order Id: {0} and \n Total Price: {1}.\n ";
        private const string OrderStatusMsg = "* Order Date: {0} Total Price: {1} Status: {2} \n ";

        private const string QuitMsg = "Bye {0}. Thanks for using OAChatBot.";

        public override async Task StartAsync(IDialogContext context)
        {
            var userName = "";

            try
            {
                var msg = context.MakeMessage();
                userName = msg.To.Name;

                var userCntx = new UserContext()
                {
                    UserId = userName
                };
                context.UserData.SetValue("userContext", userCntx);
            }
            catch (Exception)
            { }

            var hasQuit = false;
            if (context.PerUserInConversationData.TryGetValue("HasQuit", out hasQuit) && hasQuit)
            {
                UserContext userCntx = null;
                context.UserData.TryGetValue("userContext", out userCntx);

                //await context.PostAsyncCustom(string.Format(QuitMsg, (userCntx != null) ? userCntx.UserId : ""));
                context.PerUserInConversationData.SetValue("HasQuit", false);
            }
            else
            {
                context.PerUserInConversationData.SetValue("IsStart", true);
                await
                    context.PostAsyncCustom(
                        $"Hi!! {DateTime.UtcNow.GetWishBasedOnTime()} {userName}. \n How may I assist you?");// {GuideMsg}");
            }

            await base.StartAsync(context);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            context.PerUserInConversationData.SetValue("UserInput", result.Query);

            var isStart = false;
            context.PerUserInConversationData.TryGetValue("IsStart", out isStart);
            context.PerUserInConversationData.SetValue("IsStart", false);

            if ((!isStart)
                && (result.Query.ToLowerInvariant().Contains("hi") || result.Query.ToLowerInvariant().Contains("hello") || result.Query.ToLowerInvariant().Contains("/start")))
            {
                UserContext userCntx = null;
                context.UserData.TryGetValue("userContext", out userCntx);
                await context.PostAsyncCustom($"Hi!! welcome back {userCntx.UserId}.\n How may I assist you? \n");//{GuideMsg}");
                context.Wait(MessageReceived);
                return;
            }

            if (string.Compare(result.Query, "done", StringComparison.OrdinalIgnoreCase) == 0)
            {
                UserContext userCntx = null;
                context.UserData.TryGetValue<UserContext>("userContext", out userCntx);

                var order = await ServiceHandler.CreateNewOrder(userCntx);

                if (order == null)
                    await context.PostAsyncCustom("Sorry, error creating the new order.");
                else
                    await context.PostAsyncCustom(string.Format(OrderSuccessMsg, order.OrderID, order.TotalPrice));

                context.Wait(MessageReceived);
                return;
            }

            if (string.Compare(result.Query, "status", StringComparison.OrdinalIgnoreCase) == 0)
            {
                await CheckOrderStatus(context, result);
                context.Wait(MessageReceived);
                return;
            }
            if (string.Compare(result.Query, "offers", StringComparison.OrdinalIgnoreCase) == 0)
            {
                await context.PostAsyncCustom("TBD");
                context.Wait(MessageReceived);
                return;
            }
            if (string.Compare(result.Query, "return", StringComparison.OrdinalIgnoreCase) == 0)
            {
                try
                {
                    await ReturnItems(context);
                }
                catch (Exception ex)
                {
                    await context.PostAsyncCustom(OperationsErrorMsg);
                    context.Wait(MessageReceived);
                }

                return;
            }
            if ((string.Compare(result.Query, "quit", StringComparison.OrdinalIgnoreCase) == 0) ||
                (string.Compare(result.Query, "exit", StringComparison.OrdinalIgnoreCase) == 0))
            {
                await WhenUserQuits(context);
                //context.Done("quit");
                //context.PerUserInConversationData.SetValue("HasQuit", true);
                return;
            }

            if (!isStart)
            {
                string message = "Sorry, I did not understand.";//\n You can use this bot to: \n ";// + GuideMsg;
                await context.PostAsyncCustom(message);
            }

            context.Wait(MessageReceived);
        }

        [LuisIntent("AddToCart")]
        public async Task AddToCart1(IDialogContext context, LuisResult result)
        {
            context.PerUserInConversationData.SetValue("UserInput", result.Query);

            context.UserData.RemoveValue("LuisResult");
            context.PerUserInConversationData.SetValue<LuisResult>("LuisResult", result);
            context.PerUserInConversationData.SetValue<LuisResult>("NewLuisResult", result);

            try
            {
                //await AddItemsToCart(context, result);
                var addDialog = new AddItemDialog();
                context.Call(addDialog, AddItemComplete);
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationsErrorMsg);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("GetOrderStatus")]
        public async Task GetOrderStatus(IDialogContext context, LuisResult result)
        {
            try
            {
                //await CheckOrderStatus(context, result);
                //context.Wait(MessageReceived);
                var deliveryDialog = new DeliveryStatusDialog();
                context.Call(deliveryDialog, DelStatusComplete);
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationsErrorMsg);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("GetClaimStatus")]
        public async Task GetClaimStatus(IDialogContext context, LuisResult result)
        {
            try
            {
                //await CheckOrderStatus(context, result);
                //context.Wait(MessageReceived);
                var claimDialog = new ClaimStatusDialog();
                context.Call(claimDialog, DelStatusComplete);
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationsErrorMsg);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("CancelOrder")]
        public async Task CancelOrder(IDialogContext context, LuisResult result)
        {
            try
            {
                await CancelTheOrder(context, result);
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationsErrorMsg);
                context.Wait(MessageReceived);
            }
        }

        [LuisIntent("ReturnItem")]
        public async Task ReturnItem(IDialogContext context, LuisResult result)
        {
            try
            {
                //EntityRecommendation entity = null;
                //var product = "";

                //if (result.TryFindEntity("Product", out entity))
                //    product = entity.Entity;

                var returnItemDialog = new ReturnItemDialog(result.Query);
                context.Call(returnItemDialog, ReturnDamagedItemComplete);
                //await ReturnDamagedItem(context, result);
            }
            catch (Exception ex)
            {
                await context.PostAsyncCustom(OperationsErrorMsg);
                context.Wait(MessageReceived);
            }

            //context.Wait(MessageReceived);
        }

        [LuisIntent("RepeatOrderItem")]
        public async Task RepeatOrderItem(IDialogContext context, LuisResult result)
        {
            context.PerUserInConversationData.SetValue("UserInput", result.Query);
            var prevResult = context.PerUserInConversationData.Get<LuisResult>("LuisResult");

            if (prevResult != null)
            {
                EntityRecommendation recomm = null;
                EntityRecommendation newRecomm = null;

                if (prevResult.TryFindEntity("Quantity", out recomm))
                {
                    var qty = recomm;

                    if (result.TryFindEntity("Quantity", out newRecomm))
                        qty = newRecomm;
                }

                context.PerUserInConversationData.SetValue("NewLuisResult", prevResult);

                var addDialog = new AddItemDialog();
                context.Call(addDialog, AddItemComplete);

                //await AddItemsToCart(context, prevResult);
            }
            else
                await context.PostAsyncCustom("Sorry, could not find the previously ordered item.");
        }


        #region Private Methods

        private async Task ReturnItems(IDialogContext context)
        {
            UserContext userCntx = null;
            context.UserData.TryGetValue<UserContext>("userContext", out userCntx);
            var recentOrders = await ServiceHandler.GetLatestOrdersForUser(userCntx);


            var orderFrm = new ReturnOrderForm() { UserContext = userCntx, RecentOrders = recentOrders };
            var form = FormDialog.FromForm(orderFrm.BuildForm, options: FormOptions.PromptInStart);
            context.Call(form, ReturnItemComplete);
        }

        private async Task ReturnItemComplete(IDialogContext context, IAwaitable<ReturnOrderForm> result)
        {
            var orderForm = await result;
            context.Wait(MessageReceived);
        }





        //private async Task ReturnDamagedItem(IDialogContext context, LuisResult result)
        //{
        //    await context.PostAsyncCustom("I am so sorry for this.");

        //    EntityRecommendation entity = null;

        //    if (result.TryFindEntity("Product", out entity))
        //    {
        //        var product = await GetProductDetails(context, entity.Entity);

        //        PromptDialog.Confirm(context, UserProductConfirm, $"I can see {product}. Is this the one?", promptStyle: PromptStyle.Inline);
        //    }
        //    else
        //    {
        //        PromptDialog.Text(context, ProductDetailComplete, "Can you please provide me the product detail?");
        //    }
        //}

        //private async Task ProductDetailComplete(IDialogContext context, IAwaitable<string> result)
        //{
        //    var enteredProduct = await result;

        //    await context.PostAsyncCustom($"Thank you.");

        //    var product = await GetProductDetails(context, enteredProduct);

        //    PromptDialog.Confirm(context, UserProductConfirm, $"I can see {product}. Is this the one?", promptStyle: PromptStyle.Inline);
        //}

        //private async Task UserProductConfirm(IDialogContext context, IAwaitable<bool> result)
        //{
        //    var isCorrect = await result;

        //    if (isCorrect)
        //    {
        //        var refundOptions = new List<string> { "Refund", "Replacement" };
        //        PromptDialog.Choice(context, RefundOptionsComplete, refundOptions, "Shall we send you a replacement, or shall I process a refund?");
        //    }
        //    else
        //        PromptDialog.Text(context, ProductDetailComplete, "Can you please provide me the product detail again?");
        //}

        //private async Task RefundOptionsComplete(IDialogContext context, IAwaitable<string> result)
        //{
        //    var selOption = await result;

        //    if (String.Compare(selOption, "Refund", StringComparison.OrdinalIgnoreCase) == 0)
        //        await context.PostAsyncCustom("Will be replaced within 3 working days.");
        //    else
        //        await context.PostAsyncCustom("Will be refunded within 3 working days.");

        //    PromptDialog.Confirm(context, FurtherAssistanceComplete, "Is there anything else we can assit you with?", promptStyle: PromptStyle.None);
        //}

        //private async Task<string> GetProductDetails(IDialogContext context, string product)
        //{
        //    UserContext userCntx = null;
        //    context.UserData.TryGetValue<UserContext>("userContext", out userCntx);
        //    var ordritems = await ServiceHandler.GetOrderItemForUser(product, userCntx);

        //    if (ordritems == null) return product;
        //    else
        //    {
        //        var orderItem = ordritems.FirstOrDefault();
        //        if (orderItem == null) return product;
        //        else return $"Item: {orderItem.ItemName} {orderItem.Quantity} Rs.{orderItem.UnitPrice}, \n Ordered On: {orderItem.CreatedDt.ToString("yy-MM-dd HH:mm:ss")}";
        //    }
        //}

        #region Order Status
        private async Task CheckOrderStatus(IDialogContext context, LuisResult result = null)
        {
            //PromptDialog.Text(context, OrderInputComplete, "Please enter: \n * Order No. or \n * Order Date (in mm/dd/yyyy format) or \n * type **last** to know about last order.", "Please provide the details.");
            UserContext userCntx = null;
            context.UserData.TryGetValue<UserContext>("userContext", out userCntx);

            var orderRequest = GetSearchOrderRequest(result);

            if (orderRequest == null || (orderRequest.orderfromdate == null && orderRequest.orderid == null))
                await context.PostAsyncCustom("Fetching the status of last 5 orders..");

            var orders = await ServiceHandler.GetOrderStatus(orderRequest, userCntx);

            if (orders == null)
            {
                await context.PostAsyncCustom("Could not find any orders for the input. Fetching the status of last 5 orders..");
                orders = await ServiceHandler.GetOrderStatus(null, userCntx);
            }

            if (orders == null)
            {
                await context.PostAsyncCustom("Sorry, could not find any orders.");
            }
            else
            {
                await ShowOrderStatus(context, orders);
            }
        }

        private async Task ShowOrderStatus(IDialogContext context, List<BbOrder> orders)
        {
            var orderBuilder = new StringBuilder();

            foreach (var bbOrder in orders)
            {
                orderBuilder.AppendFormat(OrderStatusMsg, bbOrder.OrderDate, bbOrder.TotalPrice, bbOrder.Comments);
            }

            await context.PostAsyncCustom($"Status of your order: \n {orderBuilder}");
        }

        private static SearchOrderRequest GetSearchOrderRequest(LuisResult result)
        {
            var searchRequest = new SearchOrderRequest();

            EntityRecommendation entity = null;
            if (result.TryFindEntity("OrderNumber", out entity))
            {
                searchRequest.orderid = entity.Entity;
                return searchRequest;
            }

            if (result.TryFindType("builtin.datetime.date", out entity))
            {
                string orderDateString;
                entity.Resolution.TryGetValue("date", out orderDateString);

                DateTime dtInput;

                if (DateTime.TryParse(orderDateString, out dtInput))
                {
                    searchRequest.orderfromdate = dtInput.ToString("MM/dd/yyyy");
                    searchRequest.ordertodate = dtInput.AddDays(1).ToString("MM/dd/yyyy");
                }
                else
                {
                    if (!result.TryFindType("builtin.datetime.time", out entity)) return searchRequest;

                    entity.Resolution.TryGetValue("time", out orderDateString);

                    if (!DateTime.TryParse(orderDateString, out dtInput)) return searchRequest;

                    searchRequest.orderfromdate = dtInput.ToString("MM/dd/yyyy");
                    searchRequest.ordertodate = dtInput.AddDays(1).ToString("MM/dd/yyyy");
                }
            }

            return searchRequest;
        }


        //private async Task OrderInputComplete(IDialogContext context, IAwaitable<string> orderSelection)
        //{
        //    await context.PostAsyncCustom($"Your order has been shipped on {DateTime.Now.AddHours(-2).ToString("MM/dd/yyyy HH:mm")}");
        //    context.Wait(MessageReceived);
        //}

        #endregion Order Status

        #region CancelOrder

        private async Task CancelTheOrder(IDialogContext context, LuisResult result)
        {
            UserContext userCntx = null;
            context.UserData.TryGetValue<UserContext>("userContext", out userCntx);

            var orderRequest = GetSearchOrderRequest(result);

            if (orderRequest == null || (orderRequest.orderfromdate == null && orderRequest.orderid == null))
            {
                await context.PostAsyncCustom("Fetching the status of last 5 orders..");
                orderRequest = null;
            }

            List<BbOrder> orders = await ServiceHandler.GetOrderStatus(orderRequest, userCntx, true);

            //if (orders == null || orders.Count == 0)
            //{
            //    await context.PostAsyncCustom("Could not find any orders for the input. Fetching the status of last 5 orders..");
            //    orders = await ServiceHandler.GetOrderStatus(null, userCntx, true);
            //}

            if (orders == null || orders.Count == 0)
            {
                await context.PostAsyncCustom("Sorry, could not find any orders.");
            }
            else
            {
                var orderFrm = new CancelOrderForm() { UserContext = userCntx, OrdersToCancel = orders };
                var form = FormDialog.FromForm(orderFrm.BuildForm, options: FormOptions.PromptInStart);
                context.Call(form, CancelOrderComplete);

                //if (orders.Count == 1)
                //{
                //    var order = orders.FirstOrDefault();

                //    PromptDialog.Confirm(context, async (cntx, res) =>
                //    {
                //        var hasAgreed = await res;
                //        if (hasAgreed)
                //        {
                //            await ServiceHandler.DeleteOrder(orders.First().OrderID.ToString(), userCntx);
                //            await context.PostAsyncCustom($"Cancelled the order. Your money of Rs.{order.TotalPrice} will be refunded within 24 hrs.");
                //        }
                //        context.Wait(MessageReceived);
                //    }, "Are you sure you want to cancel this order?", promptStyle: PromptStyle.None);

                //}
                //else
                //{
                //    var orderFrm = new CancelOrderForm() { UserContext = userCntx, OrdersToCancel = orders };
                //    var form = FormDialog.FromForm(orderFrm.BuildForm, options: FormOptions.PromptInStart);
                //    context.Call(form, CancelOrderComplete);
                //}
            }

        }

        private async Task CancelOrderComplete(IDialogContext context, IAwaitable<CancelOrderForm> result)
        {
            var orderForm = await result;
            context.Wait(MessageReceived);
        }

        #endregion CancelOrder

        #endregion

        private async Task AddItemComplete(IDialogContext context, IAwaitable<string> result)
        {
            PromptDialog.Text(context, FurtherAssistanceComplete, "Is there anything else we can assist you with?");
        }

        private async Task DelStatusComplete(IDialogContext context, IAwaitable<string> result)
        {
            PromptDialog.Text(context, FurtherAssistanceComplete, "Is there anything else we can assist you with?");
        }

        private async Task ReturnDamagedItemComplete(IDialogContext context, IAwaitable<string> result)
        {
            PromptDialog.Text(context, FurtherAssistanceComplete, "Is there anything else we can assist you with?");
        }

        private async Task FurtherAssistanceComplete(IDialogContext context, IAwaitable<string> result)
        {
            var assistResult = await result;

            var luisClient = await LuisClient.Create().SendQuery(assistResult);

            Intent intent = null;

            if (luisClient != null && luisClient.IsTopIntent("SayNo", out intent))
            {
                await WhenUserQuits(context);
            }
            else
            {
                var hasIntent = await InvokeOtherIntent(context, luisClient);

                if (!hasIntent)
                {
                    await context.PostAsyncCustom("Let me know how may I again assist you?");
                    context.Wait(MessageReceived);
                    //this.MessageReceived()
                }
            }

            //if (assistResult.ContainsNo())
            //{
            //    await WhenUserQuits(context);
            //}
            //else
            //{
            //    await context.PostAsyncCustom("Let me know how may assist you?");
            //    context.Wait(MessageReceived);
            //}
        }

        private async Task<bool> InvokeOtherIntent(IDialogContext context, LuisResponse luisResponse)
        {
            var topIntent = luisResponse.TopIntent();
            switch (topIntent.Name)
            {
                case "AddToCart":
                    context.PerUserInConversationData.SetValue("UserInput", luisResponse.Query);

                    try
                    {
                        //await AddItemsToCart(context, result);
                        var addDialog = new AddItemDialog();
                        context.Call(addDialog, AddItemComplete);
                    }
                    catch (Exception ex)
                    {
                        await context.PostAsyncCustom(OperationsErrorMsg);
                        context.Wait(MessageReceived);
                    }

                    return true;

                case "GetOrderStatus":
                    try
                    {
                        var deliveryDialog = new DeliveryStatusDialog();
                        context.Call(deliveryDialog, DelStatusComplete);
                    }
                    catch (Exception ex)
                    {
                        await context.PostAsyncCustom(OperationsErrorMsg);
                        context.Wait(MessageReceived);
                    }
                    return true;

                case "ReturnItem":
                    try
                    {
                        var returnItemDialog = new ReturnItemDialog(luisResponse.Query);
                        context.Call(returnItemDialog, ReturnDamagedItemComplete);
                    }
                    catch (Exception ex)
                    {
                        await context.PostAsyncCustom(OperationsErrorMsg);
                        context.Wait(MessageReceived);
                    }
                    return true;
                case "RepeatOrderItem":
                    try
                    {
                        context.PerUserInConversationData.SetValue("UserInput", luisResponse.Query);
                        var repeatDialog = new AddItemDialog();
                        context.Call(repeatDialog, AddItemComplete);
                    }
                    catch (Exception ex)
                    {
                        await context.PostAsyncCustom(OperationsErrorMsg);
                        context.Wait(MessageReceived);
                    }
                    return true;
            }

            return false;
        }

        private async Task WhenUserQuits(IDialogContext context)
        {
            await context.PostAsyncCustom("Thanks for your patronage and looking forward to your continued support. Have a nice day.");
            context.Done("quit");
            context.PerUserInConversationData.SetValue("HasQuit", true);
        }
    }
}