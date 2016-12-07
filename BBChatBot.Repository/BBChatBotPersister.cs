

using System;
using System.Collections.Generic;
using System.Linq;

namespace OAChatBot.Repository
{
    public class BBChatBotPersister
    {
        public List<Product> GetProducts(string productName, string qty, string unit)
        {
            try
            {
                var cntx = new Entities();

                var prods = cntx.Products.Where(prod => prod.ProductName.Contains(productName));

                if (qty != null)
                    prods.Where(quantity => quantity.QuantityPerUnit.Contains(qty));


                if (unit != null)
                    prods.Where(prodUnit => prodUnit.QuantityPerUnit.Contains(unit));

                return prods.ToList();
            }
            catch (System.Exception ex)
            {
                return null;
            }

        }

        public int AddItemToCart(BBShoppingCart cart)
        {
            try
            {
                var cntx = new Entities();
                //var cart = new BBShoppingCart();
                //cart.ItemName = itemName;
                //cart.Quantity = qty;
                //cart.Unit = unit;
                cart.CreatedDt = DateTime.Now;
                cart.UpdatedDt = DateTime.Now;
                cntx.BBShoppingCarts.Add(cart);

                return cntx.SaveChanges();
            }
            catch (System.Exception ex)
            {
                return 0;
            }

        }

        public void DeleteCart()
        {
            var cntx = new Entities();
            var cartItems = cntx.BBShoppingCarts.ToList();

            if (cartItems == null) return;

            foreach (var item in cartItems)
            {
                cntx.BBShoppingCarts.Remove(item);
            }

            cntx.SaveChanges();
        }

        public Order CreateNewOrder()
        {
            DeleteCart();

            return new Order
            {
                OrderID = 1000211,
                OrderDate = DateTime.Now,
                RequiredDate = DateTime.Now.AddDays(1),
                ShippedDate = DateTime.Now.AddHours(2),
                Freight = (decimal)1211.00,
            };
        }
    }
}
