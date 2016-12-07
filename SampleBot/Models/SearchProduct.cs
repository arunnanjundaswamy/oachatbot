using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAChatBot.Models
{
    [Serializable]
    public class SearchProduct
    {
        public string ProductName { get; set; }
        public string Quantity { get; set; }
        public string Unit { get; set; }
        public string number { get; set; }

    }
}