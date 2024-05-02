using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class TotalSale
    {
        public int thang { get; set; }
        public decimal? tong { get; set; }

        public string CategoryName { get; set; }
    }
}