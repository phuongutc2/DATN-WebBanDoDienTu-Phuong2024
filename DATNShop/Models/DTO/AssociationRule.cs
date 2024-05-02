using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.DTO
{
    public class AssociationRule
    {
        public string Label { get; set; }
        public double Confidance { get; set; }
        public double Support { get; set; }
    }
}