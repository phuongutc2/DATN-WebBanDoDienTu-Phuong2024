using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class CartDTO
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public long OrderDetail_ID { get; set; }
        public int actual_number { get; set; }
        public long? Seller_ID { get; set; }
    }
}