using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DATNShop.Models.DTO
{
    public class UserDTO
    {
        public long User_ID { get; set; }
        public long Order_ID { get; set; }
        public string Fullname { get; set; }
        public string Category_Name { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> TotalQuantity { get; set; }
        public Nullable<decimal> TotalMoney { get; set; }
        public Nullable<DateTime> LastDayBought { get; set; }
        public Nullable<long> RoleID { get; set; }
        public Nullable<bool> Status { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}