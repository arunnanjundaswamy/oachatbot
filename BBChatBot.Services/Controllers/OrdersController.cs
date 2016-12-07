using OAChatBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OAChatBot.Services.Controllers
{
    public class OrdersController : ApiController
    {
        [Route("api/orders")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage CreateNewOrder()
        {
            var persister = new BBChatBotPersister();
            persister.DeleteCart();

            var newOrder = persister.CreateNewOrder();

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, newOrder);
            return response;
        }
    }
}
