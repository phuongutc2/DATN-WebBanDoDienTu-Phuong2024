using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class OrderDTO
    {
        public long Order_ID { get; set; }
        public string Order_Code { get; set; }
        public string Product_Name { get; set; }
        public Nullable<decimal> TotalMoney { get; set; }
        public int? Quantity { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<long> User_ID { get; set; }
        public Nullable<System.DateTime> ShipDate { get; set; }
        public Nullable<System.DateTime> CancerDate { get; set; }
        public Nullable<System.DateTime> PaidDate { get; set; }
        public int? Payment { get; set; }
    }
}