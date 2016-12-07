using OAChatBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OAChatBot.Services.Controllers
{
    public class ShoppingCartController : ApiController
    {
        [Route("api/shoppingcart")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage AddItemToCart(BBShoppingCart cart)
        {
            var persister = new BBChatBotPersister();
            int result = persister.AddItemToCart(cart);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, result);
            return response;
        }

        [Route("api/shoppingcart")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage DeleteCart()
        {
            var persister = new BBChatBotPersister();
            persister.DeleteCart();

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
    }
}
