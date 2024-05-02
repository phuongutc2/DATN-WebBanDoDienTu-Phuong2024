using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class InwardDTO
    {
        public long InwardDetail_ID { get; set; }
        public Nullable<long> Inward_ID { get; set; }
        public Nullable<long> Product_ID { get; set; }
        public Nullable<long> Serial_Type_ID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<System.DateTime> Createdate { get; set; }
        public Nullable<long> Provider_ID { get; set; }
        public string Product_Name { get; set; }
        public string Provider_Name { get; set; }
        public string Product_Image { get; set; }

        public Nullable<long> TotalQuantity { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
    }
}