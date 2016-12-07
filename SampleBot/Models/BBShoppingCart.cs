using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAChatBot.Models
{
    [Serializable]
    public class BbShoppingCart
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public string Unit { get; set; }
        public Nullable<int> Sequence { get; set; }
        public string UserId { get; set; }
        public System.DateTime CreatedDt { get; set; }
        public Nullable<System.DateTime> UpdatedDt { get; set; }
        public string Quantity { get; set; }
        public int? ProductId { get; set; }
    }
}