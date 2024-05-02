using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class Order_DetailDTO
    {
        public long ID { get; set; }
        public Nullable<long> Product_ID { get; set; }
        public Nullable<long> SerialType_ID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<long> Order_ID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<double> DiscountAmount { get; set; }
        public Nullable<double> DiscountRate { get; set; }
        public Nullable<int> Amount { get; set; }
        public bool? Pament { get; set; }

        public string Order_Code { get; set; }
        public string Product_Name { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<DateTime> ShipDate { get; set; }
        public Nullable<int> Status { get; set; }
    }
}