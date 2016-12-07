using OAChatBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OAChatBot.Services
{
    public class ServiceHandler
    {
        private const string ProductAddSuccessMsg = "Added {0} to cart. Continue adding new item or type **done** to complete the order.";
        private const string ProductAddFailMsg = "Sorry, some error in adding items to the cart. Add new item \n or \n type **done** to complete the order.";

        private static readonly string BotServiceBaseUrl = ConfigurationManager.AppSettings["BBChatServiceBaseUrl"];

        #region Products
        public static async Task<List<BbProduct>> GetProducts(string productName, string qty, string unit, bool withOffersOnly = false)
        {
            try
            {
                //string requestUrl = $"api/products/{productName}/{qty}/{unit}";
                string requestUrl = $"{BotServiceBaseUrl}api/bot/bbproducts?productname=ilike.*{productName}*";

                using (HttpClient client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(_botServiceBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    string content = await response.Content.ReadAsStringAsync();

                    List<BbProduct> result = JsonConvert.DeserializeObject<List<BbProduct>>(content);
                    return result;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<string> PersisitToCart(BbProduct prod, UserContext userCntxt)
        {
            var requestCart = new AddShoppingCartRequest
            {
                itemname = prod.ProductName,
                createddt = DateTime.Now,
                quantity = prod.QuantityPerUnit,
                sequence = null,
                unit = "1",
                unitprice = prod.UnitPrice,
                updateddt = DateTime.Now,
                userid = (userCntxt != null) ? userCntxt.UserId : "1",
                productid = prod.ProductID
            };

            //string requestUrl = $"api/shoppingcart";
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bbshoppingcart";

            using (HttpClient client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string cartData = JsonConvert.SerializeObject(requestCart);
                var stringContent = new StringContent(cartData, System.Text.Encoding.UTF8, "application/json");// "{ \"ItemName\": \"Lux1234\" }", System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(requestUrl, stringContent);

                return (response.IsSuccessStatusCode) ? string.Format(ProductAddSuccessMsg, prod.ProductName) : ProductAddFailMsg;
                //if (response.IsSuccessStatusCode)
                //{
                //    //string content = await response.Content.ReadAsStringAsync();
                //    //int result = JsonConvert.DeserializeObject<int>(content);
                //    //return result;
                //    //return 1;

                //}
                //return 0;
            }

        }
        #endregion Products

        #region Cart
        private static async Task<List<BbShoppingCart>> GetCartItemsForUser(UserContext userContext)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bbshoppingcart?userid=eq.{userContext.UserId}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(_botServiceBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync(requestUrl);
                    string content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<List<BbShoppingCart>>(content);
                    return result;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static async Task DeleteCartItems(UserContext userContext)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bbshoppingcart?userid=eq.{userContext.UserId}";

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.DeleteAsync(requestUrl);
            }
        }
        #endregion Cart

        #region Order
        public static async Task<List<BbOrder>> GetLatestOrdersForUser(UserContext userContext, short howMany = 3)
        {
            try
            {
                string requestUrl = $"{BotServiceBaseUrl}api/bot/bborders?order=orderid.desc";

                using (HttpClient client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(_botServiceBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Range", "0-3");
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(requestUrl);
                    string content = await response.Content.ReadAsStringAsync();

                    List<BbOrder> result = JsonConvert.DeserializeObject<List<BbOrder>>(content);
                    return result;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<List<BbOrderItem>> GetOrderItemsForOrder(int orderId)
        {
            try
            {
                string requestUrl = $"{BotServiceBaseUrl}api/bot/bborderitems?orderid=eq.{orderId}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync(requestUrl);
                    string content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<List<BbOrderItem>>(content);
                    return result;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<BbOrder> CreateNewOrder(UserContext userContext)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bborders";

            var cartItems = await GetCartItemsForUser(userContext);

            if (cartItems == null || cartItems.Count == 0) return null;

            var requestOrder = new AddOrderRequest
            {
                createddt = DateTime.Now,
                updateddt = DateTime.Now,
                shippeddate = DateTime.Now.AddMinutes(40),
                customerid = (userContext != null) ? userContext.UserId : "User1",//order.CustomerID,
                orderdate = DateTime.Now,
                comments = "Out for delivery. Will reach you in another 30 mins.",
                status = 1
                //totalprice = 2080
            };

            decimal totalPrice = 0;

            foreach (var item in cartItems)
            {
                if (item.UnitPrice.HasValue)
                    totalPrice = totalPrice + item.UnitPrice.Value;
            }
            requestOrder.totalprice = totalPrice;

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Prefer", "return=representation");

                string orderData = JsonConvert.SerializeObject(requestOrder);
                var stringContent = new StringContent(orderData, System.Text.Encoding.UTF8, "application/json");// "{ \"ItemName\": \"Lux1234\" }", System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, stringContent);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<BbOrder>(content);

                    await InsertOrderItems(result, cartItems, userContext);

                    await DeleteCartItems(userContext);

                    return result;

                }
                return null;
            }
        }

        public static async Task DeleteOrder(string orderId, UserContext userContext)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bborders?orderid=eq.{orderId}";

            var updateReq = new UpdateOrderRequest
            {
                status = 2,
                comments = $"Cancelled by the user:{userContext.UserId}",
                updateddt = DateTime.Now
            };

            var method = new HttpMethod("PATCH");
            string orderData = JsonConvert.SerializeObject(updateReq);
            var stringContent = new StringContent(orderData, System.Text.Encoding.UTF8, "application/json");// "{ \"ItemName\": \"Lux1234\" }", System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(method, requestUrl)
            {
                Content = stringContent
            };

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.SendAsync(request);
            }
        }

        private static async Task InsertOrderItems(BbOrder bbOrder, List<BbShoppingCart> cartItems, UserContext userContext)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bborderitems";

            var orderItems = new List<OrderItemRequest>();

            if (cartItems != null)
            {
                foreach (var item in cartItems)
                {
                    var requestOrderItem = new OrderItemRequest()
                    {
                        itemname = item.ItemName,
                        createddt = DateTime.Now,
                        quantity = item.Quantity,
                        unit = item.Unit,
                        updateddt = DateTime.Now,
                        unitprice = item.UnitPrice,
                        userid = (userContext != null) ? userContext.UserId : "User1",//order.CustomerID,
                        productid = item.ProductId,
                        orderid = bbOrder.OrderID
                    };
                    orderItems.Add(requestOrderItem);
                }
            }

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Prefer", "return=representation");

                string orderData = JsonConvert.SerializeObject(orderItems);
                var stringContent = new StringContent(orderData, System.Text.Encoding.UTF8, "application/json");// "{ \"ItemName\": \"Lux1234\" }", System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, stringContent);

                //if (response.IsSuccessStatusCode)
                //{
                //string content = await response.Content.ReadAsStringAsync();
                //JsonConvert.DeserializeObject<BbOrder>(content);
                //}
            }
        }

        public static async Task DeleteOrderItem(int orderItemId)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bborderitems?orderitemid=eq.{orderItemId}";

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                await client.DeleteAsync(requestUrl);
            }
        }

        public static async Task<List<BbOrder>> GetOrderStatus(SearchOrderRequest orderRequest, UserContext userContext, bool onlyActive = false)
        {
            try
            {
                bool hasRequest = false;
                string requestUrl = $"{BotServiceBaseUrl}api/bot/bborders?customerid=eq.{userContext.UserId}&order=orderid.desc";

                if (onlyActive)
                {
                    requestUrl = $"{requestUrl}&status=not.eq.2";
                }

                if (orderRequest != null)
                {
                    if (orderRequest.orderfromdate != null && orderRequest.ordertodate != null)
                    {
                        hasRequest = true;
                        requestUrl = $"{requestUrl}&orderdate=gte.{orderRequest.orderfromdate}&orderdate=lt.{orderRequest.ordertodate}";
                    }

                    if (!string.IsNullOrEmpty(orderRequest.orderid))
                    {
                        hasRequest = true;
                        requestUrl = $"{requestUrl}&orderid=eq.{orderRequest.orderid}";
                    }
                }

                using (var client = new HttpClient())
                {
                    //client.BaseAddress = new Uri(_botServiceBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (!hasRequest)
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Range", "0-4");

                    var response = await client.GetAsync(requestUrl);
                    var content = await response.Content.ReadAsStringAsync();

                    return JsonConvert.DeserializeObject<List<BbOrder>>(content);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<List<BbOrderItem>> GetOrderItemForUser(string product, UserContext userCntx)
        {
            try
            {
                string requestUrl = $"{BotServiceBaseUrl}api/bot/bborderitems?userid=eq.{userCntx.UserId}&itemname=ilike.*{product}*";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await client.GetAsync(requestUrl);
                    string content = await response.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<List<BbOrderItem>>(content);
                    return result;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion Order

        public static async Task TraceMessage(BbConversation coversation)
        {
            string requestUrl = $"{BotServiceBaseUrl}api/bot/bbconversation";

            using (var client = new HttpClient())
            {
                //client.BaseAddress = new Uri(_botServiceBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string orderData = JsonConvert.SerializeObject(coversation);
                var stringContent = new StringContent(orderData, System.Text.Encoding.UTF8, "application/json");// "{ \"ItemName\": \"Lux1234\" }", System.Text.Encoding.UTF8, "application/json");

                var response = await client.PostAsync(requestUrl, stringContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error in saving the conversation");
                }
            }
        }
    }

    [Serializable]
    public class AddOrderRequest
    {
        public string customerid { get; set; }
        public decimal? totalprice { get; set; }
        public DateTime? orderdate { get; set; }
        public DateTime? shippeddate { get; set; }
        public DateTime? createddt { get; set; }
        public DateTime? updateddt { get; set; }
        public short status { get; set; }
        public string comments { get; set; }
    }

    [Serializable]
    public class UpdateOrderRequest
    {
        public short status { get; set; }
        public string comments { get; set; }
        public DateTime? updateddt { get; set; }
    }

    [Serializable]
    public class SearchOrderRequest
    {
        public string orderid { get; set; }
        public string orderfromdate { get; set; }
        public string ordertodate { get; set; }
    }

    [Serializable]
    public class OrderItemRequest
    {
        public string itemname { get; set; }
        public decimal? unitprice { get; set; }
        public string unit { get; set; }
        public string userid { get; set; }
        public DateTime createddt { get; set; }
        public DateTime? updateddt { get; set; }
        public string quantity { get; set; }
        public int? orderid { get; set; }
        public int? productid { get; set; }
    }

    [Serializable]
    public class AddShoppingCartRequest
    {
        public string itemname { get; set; }
        public decimal? unitprice { get; set; }
        public string unit { get; set; }
        public int? sequence { get; set; }
        public string userid { get; set; }
        public DateTime createddt { get; set; }
        public DateTime? updateddt { get; set; }
        public string quantity { get; set; }
        public int? productid { get; set; }
    }
}