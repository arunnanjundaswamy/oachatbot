using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using OAChatBot.Extensions;
using OAChatBot.Forms;
using OAChatBot.Luis;
using OAChatBot.Models;
using OAChatBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace OAChatBot.Dialogs
{
    [Serializable]
    public class AddItemDialog : IDialog<string>
    {
        private List<SearchProduct> _luisProducts;
        private short _currentProdCnt = 0;
        private const string NoMatchingProdMsg = "Sorry, could not find the Product: {0}";
        private const string OrderSuccessMsg =
           "Generated a new order with \n Order Id: {0} and \n Total Price: {1}.\n ";

        public async Task StartAsync(IDialogContext context)
        {
            await AddItemsToCart(context);
        }

        private async Task AddItemsToCart(IDialogContext context)
        {
            string usrInput = "";
            context.PerUserInConversationData.TryGetValue("UserInput", out usrInput);

            _luisProducts = await GetSearchedProducts(context, usrInput);

            if (_luisProducts != null && _luisProducts.Count > 0)
            {
                await ProcessItemAddition(context);//To be added
            }
            else
            {
                //await context.PostAsyncCustom("Sorry, could not find the Product!!");
                await context.PostAsyncCustom("Can you provide the product details you want to purchase?");
                context.Wait(MessageReceived);
                //context.Done("close");
            }
        }

        private async Task ProcessItemAddition(IDialogContext context)
        {
            var productSearched = _luisProducts[_currentProdCnt];

            var prodsList = await ServiceHandler.GetProducts(productSearched.ProductName, null, null);

            if (prodsList == null || prodsList.Count == 0)
            {
                await context.PostAsyncCustom(string.Format(NoMatchingProdMsg, productSearched.ProductName));

                if (_currentProdCnt < (_luisProducts.Count - 1))
                {
                    _currentProdCnt++;
                    await ProcessItemAddition(context);
                    return;
                }

                //context.Done("close");
                context.Wait(MessageReceived);
            }
            else//check if the quantity matches
            {
                var prodsWithQty = prodsList.Where(prods => (prods.QuantityPerUnit != null && productSearched.Quantity != null)
                   && (prods.QuantityPerUnit.Contains(productSearched.Quantity) ||
                   (prods.QuantityPerUnit.Contains(productSearched.Quantity)))).ToList();

                List<BbProduct> productsToDisplay = (prodsWithQty == null || prodsWithQty.Count == 0) ? prodsList : prodsWithQty;

                if (productsToDisplay.Count > 1)
                {
                    HandleMultiProducts(context, productsToDisplay);
                }
                else
                {
                    var prod = productsToDisplay.FirstOrDefault();

                    if (prod == null)
                    {
                        await context.PostAsyncCustom(string.Format(NoMatchingProdMsg, productSearched.ProductName));

                        if (_currentProdCnt < (_luisProducts.Count - 1))
                        {
                            _currentProdCnt++;
                            await ProcessItemAddition(context);
                            return;
                        }

                        context.Wait(MessageReceived);
                        //context.Done("close");
                        return;
                    }

                    UserContext userContext = null;
                    context.UserData.TryGetValue("userContext", out userContext);

                    var message = await ServiceHandler.PersisitToCart(prod, userContext);
                    await context.PostAsyncCustom(message);

                    if (_currentProdCnt < (_luisProducts.Count - 1))
                    {
                        _currentProdCnt++;
                        await ProcessItemAddition(context);
                        return;
                    }

                    //context.Done("close");
                    context.Wait(MessageReceived);

                }
            }
        }

        private void HandleMultiProducts(IDialogContext context, List<BbProduct> prodsList)
        {
            var promptData = new List<string>();
            var promptProducts = new Dictionary<string, BbProduct>();

            foreach (var prod in prodsList)
            {
                var product = $"{prod.ProductName} {prod.QuantityPerUnit} Rs.{prod.UnitPrice}";

                promptData.Add(product);
                promptProducts.Add(product, prod);
            }
            context.UserData.SetValue("promptData", promptProducts);

            var prodFrm = new MultiProductForm { Products = promptData };
            var form = FormDialog.FromForm(prodFrm.BuildForm, options: FormOptions.PromptInStart);
            context.Call(form, SelectionComplete);
        }

        private async Task SelectionComplete(IDialogContext context, IAwaitable<MultiProductForm> result)
        {
            if (_currentProdCnt < (_luisProducts.Count - 1))
            {
                _currentProdCnt++;
                await ProcessItemAddition(context);
                return;
            }

            //context.Done("close");
            context.Wait(MessageReceived);
        }

        private async Task<List<SearchProduct>> GetSearchedProducts(IDialogContext context, string userQuery)
        {
            LuisResult result = null;
            context.PerUserInConversationData.TryGetValue("NewLuisResult", out result);

            var luisResp = await LuisClient.Create().SendQuery(userQuery);

            if (luisResp == null) return null;

            //var entities = new List<EntityRecommendation>(result.Entities);
            //var productEntities = entities.Where((ent) => ent.Type == "Product").ToList();

            var productEntities = luisResp.FindType("Product");

            if (productEntities != null && (productEntities.Count > 1))
            {
                var splist = new List<SearchProduct>();

                foreach (var entity in luisResp.Entities)
                {
                    if (entity.Type == "Product")
                    {
                        var prod = new SearchProduct { ProductName = entity.Value };
                        splist.Add(prod);
                    }
                }
                return splist;
            }

            if (productEntities != null && productEntities.Count > 0)
            {
                var sProd = new SearchProduct();

                foreach (var entity in luisResp.Entities)
                {
                    if (entity.Type == "Product")
                        sProd.ProductName = entity.Value;
                    if (entity.Type == "Quantity")
                        sProd.Quantity = entity.Value;
                    if (entity.Type == "Unit")
                        sProd.Unit = entity.Value;
                    if (entity.Type == "built.number")
                        sProd.number = entity.Value;
                }

                return new List<SearchProduct> { sProd };
            }

            return null;
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<Message> result)
        {
            var userInput = await result;

            if (userInput != null && string.Compare(userInput.Text, "done", true) == 0)
            {
                UserContext userCntx = null;
                context.UserData.TryGetValue<UserContext>("userContext", out userCntx);

                var order = await ServiceHandler.CreateNewOrder(userCntx);

                if (order == null)
                    await context.PostAsyncCustom("Sorry, error creating the new order.");
                else
                    await context.PostAsyncCustom(string.Format(OrderSuccessMsg, order.OrderID, order.TotalPrice));

                context.Done("close");
            }
            else
            {
                context.PerUserInConversationData.SetValue("UserInput", userInput != null ? userInput.Text : "");
                await AddItemsToCart(context);
            }
        }
    }
}