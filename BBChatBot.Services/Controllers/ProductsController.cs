using OAChatBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OAChatBot.Services.Controllers
{
    public class ProductsController : ApiController
    {
        [Route("api/products/{productName:alpha}/{qty?}/{unit?}")]
        public HttpResponseMessage GetAllProducts(string productName, string qty = null, string unit = null)
        {
            var persister = new BBChatBotPersister();
            var prods = persister.GetProducts(productName, qty, unit);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, prods);
            return response;
        }
    }
}
