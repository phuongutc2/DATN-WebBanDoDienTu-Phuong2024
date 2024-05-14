using Microsoft.AspNetCore.Mvc;
using PagedList;
using ShoppingOnline.Models.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Controllers
{
    public class HomeController : System.Web.Mvc.Controller
    {
        public System.Web.Mvc.ActionResult Index()
        {
            var model = new ProductBusiness();
            ViewBag.LstProduct = model.getAll();
        
            ViewBag.lstProductRecommend_1 = model.getProductRecommend();
            ViewBag.lstProductRecommend_2 = model.getProductRecommend();

            ViewBag.lstCategory = model.GetCategories();
            ViewBag.lstProductAll = model.getAllProduct();
            return View();
        }

        public System.Web.Mvc.ActionResult GetProductByCategory(string Metatitle, long ID, string order = null, int page = 1, int pagesize = 6)
        {
            var model = new ProductBusiness().getProductByCategory(ID, page, pagesize, order);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            ViewBag.Category = new ProductBusiness().FindCategory(ID);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            ViewBag.lstCategory = new ProductBusiness().GetCategories();
            ViewBag.orderby = order; 
            return View(model);
        }

        //Load menu
        [ChildActionOnly]
        public System.Web.Mvc.PartialViewResult MainMenu()
        {
            ViewBag.lstCategory = new ProductBusiness().GetCategories();
            return PartialView();
        }

        [ChildActionOnly]
        public System.Web.Mvc.PartialViewResult Navigation()
        {
            ViewBag.lstCategory = new ProductBusiness().GetCategories();
            return PartialView();
        }
    }
}