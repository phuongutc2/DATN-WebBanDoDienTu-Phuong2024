using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class ProductDTO
    {

        public long ID { get; set; }
        public string Product_Code { get; set; }
        public string Product_Name { get; set; }
        public string Metatitle { get; set; }
        public string Object { get; set; }
        public Nullable<decimal> Promotion_Price { get; set; }
        public string Image { get; set; }
        public string Desription { get; set; }
        public string Configuration { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<long> Category_ID { get; set; }
        public Nullable<long> Provider_ID { get; set; }
        public Nullable<bool> Status { get; set; }

        public string Category_Name { get; set; }
        public string Seller_Name { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string Image_Detail { get; set; }
        public Nullable<decimal> Price { get; set; }
    }
}