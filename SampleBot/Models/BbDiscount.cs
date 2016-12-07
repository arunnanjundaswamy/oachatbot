using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAChatBot.Models
{
    public class BbDiscount
    {
        public int DiscountId { get; set; }
        public string Name { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTill { get; set; }
        public string ImageUrl { get; set; }
    }
}