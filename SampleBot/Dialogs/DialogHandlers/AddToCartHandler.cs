using OAChatBot.Models;
using OAChatBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OAChatBot.Dialogs.DialogHandlers
{
    //[Serializable]
    //public class AddToCartHandler
    //{
    //    private const string MultiProductMsg = "There are multiple product available.\nPlease choose any one (can enter the number on the left side) or type cancel.";
    //    private const string ProductAddSuccessMsg = "Successfully added {0} to cart. Continue adding new item or type **done** to complete the order.";
    //    private const string ProductAddFailMsg = "Sorry, some error in adding items to the cart. Add new item \n or \n type **done** to complete the order.";
    //    private const string NoMatchingProdMsg = "Sorry, could not find the Product: {0}";
    //    private const string ProductSelectionCancelled = "You have cancelled the selection. Please add new item or type **done** to create an order.";

    //    public async Task AddToCart(IDialogContext context, LuisResult result, ResumeAfter<Message> messageReceived)
    //    {
    //        var searchProducts = GetSearchedProducts(result);

    //        if (searchProducts != null && searchProducts.Count > 0)
    //        {
    //            //await HandleSingleInputProduct(context, searchProducts);
    //            //await HandleMultiInputProducts(context, searchProducts);

    //            foreach (var productSearched in searchProducts)
    //            {
    //                var prodsList = await ServiceHandler.GetProducts(productSearched.ProductName, null, null);

    //                if (prodsList == null || prodsList.Count == 0)
    //                {
    //                    await context.PostAsyncCustom(string.Format(NoMatchingProdMsg, productSearched.ProductName));
    //                    continue;
    //                }
    //                else//check if the quantity matches
    //                {
    //                    var prodsWithQty = prodsList.Where(prods => (prods.QuantityPerUnit != null && productSearched.Quantity != null)
    //                       && (prods.QuantityPerUnit.Contains(productSearched.Quantity) ||
    //                       (prods.QuantityPerUnit.Contains(productSearched.Quantity)))).ToList();

    //                    List<Product> productsToDisplay = null;
    //                    productsToDisplay = (prodsWithQty == null || prodsWithQty.Count == 0) ? prodsList : prodsWithQty;

    //                    var productNames = prodsList.Select(prods =>
    //                    {
    //                        var prodWithQtyPrice = (prods.QuantityPerUnit != null) ? prods.QuantityPerUnit : "1";
    //                        return $"{prods.ProductName} - {prods.QuantityPerUnit} - {prods.UnitPrice}";

    //                    }).ToList();

    //                    if (prodsList.Count > 1)
    //                    {
    //                        await HandleMultiProducts(context, prodsList, productNames);
    //                        return;
    //                    }
    //                    else
    //                    {
    //                        var prod = prodsList.FirstOrDefault();

    //                        if (prod == null)
    //                        {
    //                            await context.PostAsyncCustom(string.Format(NoMatchingProdMsg, productSearched.ProductName));
    //                            continue;
    //                        }

    //                        var message = await AddItemToCart(prod);
    //                        await context.PostAsyncCustom(message);
    //                    }
    //                }
    //            }
    //        }
    //        else
    //        {
    //            await context.PostAsyncCustom("Sorry, could not find the Product!!");
    //        }

    //        //if (botStatus == BotStatus.ShouldWait)
    //        context.Wait(messageReceived);
    //    }

    //    public async Task AddToCart(IDialogContext context, LuisResult result, LuisResult prevResult, ResumeAfter<Message> messageReceived)
    //    {
    //        if (prevResult != null)
    //        {
    //            EntityRecommendation recomm = null;
    //            EntityRecommendation newRecomm = null;

    //            if (prevResult.TryFindEntity("Quantity", out recomm))
    //            {
    //                var qty = recomm;

    //                if (result.TryFindEntity("Quantity", out newRecomm))
    //                    qty = newRecomm;
    //            }

    //            await AddToCart(context, prevResult, messageReceived);
    //        }
    //        else
    //            await context.PostAsyncCustom("Sorry, could not find the previously ordered item.");

    //    }

    //    private async Task HandleMultiProducts(IDialogContext context, List<Product> prodsList, List<string> productNames)
    //    {
    //        var prodBuilder = new List<string>();

    //        var prodCnt = 1;

    //        var promptData = new List<string>();
    //        var promptProducts = new Dictionary<string, Product>();

    //        foreach (var prod in prodsList)
    //        {
    //            //var product = $"{prodCnt} > {prod.ProductName}-{prod.QuantityPerUnit}-{prod.UnitPrice}";
    //            var product = $"{prodCnt}> {prod.ProductName} {prod.QuantityPerUnit} Rs.{prod.UnitPrice}";

    //            promptData.Add(product);
    //            promptProducts.Add(product, prod);

    //            prodBuilder.Add(product + "\n");

    //            prodCnt++;
    //        }
    //        prodBuilder.Add("c > cancel");
    //        promptData.Add("c> cancel");

    //        context.UserData.SetValue("promptData", promptProducts);

    //        //await context.PostAsyncCustom(MultiProductMsg);

    //        //PromptDialog.Choice(context, ShowOptions, new List<string> { "apple", "banana" }, "apple\nbanana123", "Please choose again.", 3, PromptStyle.PerLine);

    //        PromptDialog.Choice(context, SelectionComplete, promptData, MultiProductMsg, "Please choose again.", 3, PromptStyle.PerLine);
    //        //return BotStatus.IsPrompted;

    //    }

    //    private async Task SelectionComplete(IDialogContext context, IAwaitable<string> productSel)
    //    {
    //        var selectedProd = await productSel;

    //        if (string.Compare(selectedProd, "c", true) == 0 || string.Compare(selectedProd, "cancel", true) == 0)
    //        {
    //            await context.PostAsyncCustom(ProductSelectionCancelled);
    //            context.Done(ProductSelectionCancelled);
    //            return;// BotStatus.ShouldWait;
    //        }

    //        Dictionary<string, Product> userProds;
    //        context.UserData.TryGetValue("promptData", out userProds);

    //        if (userProds == null || !(userProds.ContainsKey(selectedProd)))
    //        {
    //            context.Done("Sorry, some error in the product selection.");
    //            return;// BotStatus.ShouldWait;
    //        }

    //        Product prod = userProds[selectedProd];// await productSel;

    //        if (prod == null)
    //        {
    //            context.Done("Sorry, something went wrong. Please select the product again.");
    //            return;// BotStatus.IsPrompted;
    //        }

    //        var prodQty = (prod.QuantityPerUnit != null) ? prod.QuantityPerUnit : "1";
    //        var prodSelected = $"{prod.ProductName}-{prodQty}";
    //        await context.PostAsyncCustom($"The product selected : **{prodSelected}**");

    //        var message = await AddItemToCart(prod);
    //        await context.PostAsyncCustom(message);
    //        //context.Done(message);
    //        //return BotStatus.ShouldWait;
    //        //context.Wait(base.Mess);
    //    }

    //    private async Task<string> AddItemToCart(Product prod)
    //    {
    //        var cart = new BBShoppingCart();
    //        cart.ItemName = prod.ProductName;
    //        cart.Quantity = prod.QuantityPerUnit;
    //        cart.UnitPrice = prod.UnitPrice;
    //        cart.UserId = 1;//Need to change this
    //        cart.UpdatedDt = DateTime.Now;
    //        cart.CreatedDt = DateTime.Now;

    //        int retCode = await ServiceHandler.AddItemToCart(cart);
    //        return (retCode != 0) ? string.Format(ProductAddSuccessMsg, prod.ProductName) : ProductAddFailMsg;
    //    }

    //    private List<SearchProduct> GetSearchedProducts(LuisResult result)
    //    {
    //        if (result == null) return null;

    //        var entities = new List<EntityRecommendation>(result.Entities);
    //        var productEntities = entities.Where((ent) => ent.Type == "Product").ToList();

    //        if (productEntities != null && (productEntities.Count > 1))
    //        {
    //            var splist = new List<SearchProduct>();

    //            foreach (var entity in result.Entities)
    //            {
    //                if (entity.Type == "Product")
    //                {
    //                    var prod = new SearchProduct();
    //                    prod.ProductName = entity.Entity;
    //                    splist.Add(prod);
    //                }
    //            }
    //            return splist;
    //        }

    //        if (productEntities != null && productEntities.Count > 0)
    //        {
    //            var sProd = new SearchProduct();

    //            foreach (var entity in result.Entities)
    //            {
    //                if (entity.Type == "Product")
    //                    sProd.ProductName = entity.Entity;
    //                if (entity.Type == "Quantity")
    //                    sProd.Quantity = entity.Entity;
    //                if (entity.Type == "Unit")
    //                    sProd.Unit = entity.Entity;
    //                if (entity.Type == "built.number")
    //                    sProd.number = entity.Entity;
    //            }

    //            return new List<SearchProduct> { sProd };
    //        }

    //        return null;
    //    }

    //    #region commented
    //    //private async Task HandleProductAddition(IDialogContext context, SearchProduct productSearched, ResumeAfter<Message> messageReceived)
    //    //{
    //    //    //var prodsList = await ServiceHandler.GetProducts(productSearched.ProductName, ((productSearched.Quantity == null) ? null : productSearched.Quantity),
    //    //    //            ((productSearched.Unit == null) ? null : productSearched.Unit));

    //    //    var prodsList = await ServiceHandler.GetProducts(productSearched.ProductName, null, null);

    //    //    if (prodsList == null || prodsList.Count == 0)
    //    //    {
    //    //        await context.PostAsyncCustom(string.Format(NoMatchingProdMsg, productSearched.ProductName));
    //    //    }
    //    //    else//check if the quantity matches
    //    //    {
    //    //        var prodsWithQty = prodsList.Where(prods => (prods.QuantityPerUnit != null && productSearched.Quantity != null)
    //    //           && (prods.QuantityPerUnit.Contains(productSearched.Quantity) ||
    //    //           (prods.QuantityPerUnit.Contains(productSearched.Quantity)))).ToList();

    //    //        List<Product> productsToDisplay = null;
    //    //        productsToDisplay = (prodsWithQty == null || prodsWithQty.Count == 0) ? prodsList : prodsWithQty;

    //    //        var productNames = prodsList.Select(prods =>
    //    //        {
    //    //            var prodWithQtyPrice = (prods.QuantityPerUnit != null) ? prods.QuantityPerUnit : "1";
    //    //            return $"{prods.ProductName} - {prods.QuantityPerUnit} - {prods.UnitPrice}";

    //    //        }).ToList();

    //    //        if (prodsList.Count > 1)
    //    //        {
    //    //            await HandleMultiProducts(context, prodsList, productNames);
    //    //        }
    //    //        else
    //    //        {
    //    //            var prod = prodsList.FirstOrDefault();

    //    //            if (prod == null)
    //    //            {
    //    //                await context.PostAsyncCustom(string.Format(NoMatchingProdMsg, productSearched.ProductName));
    //    //                return;
    //    //            }

    //    //            var message = await AddItemToCart(prod);
    //    //            await context.PostAsyncCustom(message);
    //    //        }

    //    //    }
    //    //}

    //    //private async Task HandleSingleInputProduct(IDialogContext context, List<SearchProduct> searchProducts)
    //    //{
    //    //    if (searchProducts != null && searchProducts.Count == 1)
    //    //    {
    //    //        var productInput = searchProducts.FirstOrDefault();

    //    //        await HandleProductAddition(context, productInput);
    //    //    }
    //    //}

    //    //private async Task HandleMultiInputProducts(IDialogContext context, List<SearchProduct> searchProducts)
    //    //{
    //    //    if (searchProducts != null && searchProducts.Count > 1)
    //    //    {
    //    //        foreach (var sProd in searchProducts)
    //    //        {
    //    //            await HandleProductAddition(context, sProd);
    //    //        }
    //    //    }
    //    //}

    //    #endregion commented

    //}

    
}