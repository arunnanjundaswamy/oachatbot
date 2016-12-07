using OAChatBot.Models;
using OAChatBot.Services;
using System;
using System.Threading.Tasks;

namespace OAChatBot.Repository
{
    public class CartRepository
    {
        private const string ProductAddSuccessMsg = "Successfully added {0} to cart. Continue adding new item or type **done** to complete the order.";
        private const string ProductAddFailMsg = "Sorry, some error in adding items to the cart. Add new item \n or \n type **done** to complete the order.";

        //public static async Task<string> PersisitToCart(Product prod, UserContext userContext)
        //{
        //    var cart = new BBShoppingCart
        //    {
        //        ItemName = prod.ProductName,
        //        Quantity = prod.QuantityPerUnit,
        //        UnitPrice = prod.UnitPrice,
        //        UserId = (userContext != null) ? userContext.UserId : "1",
        //        UpdatedDt = DateTime.Now,
        //        CreatedDt = DateTime.Now
        //    };
        //    //Need to change this

        //    int retCode = await ServiceHandler.AddItemToCart(cart, userContext);
        //    return (retCode != 0) ? string.Format(ProductAddSuccessMsg, prod.ProductName) : ProductAddFailMsg;
        //}
    }
}