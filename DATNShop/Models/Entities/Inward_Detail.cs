//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DATNShop.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Inward_Detail
    {
        public long ID { get; set; }
        public Nullable<long> Inward_ID { get; set; }
        public Nullable<long> Product_ID { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Amount { get; set; }
    
        public virtual Inward Inward { get; set; }
        public virtual Product Product { get; set; }
    }
}
