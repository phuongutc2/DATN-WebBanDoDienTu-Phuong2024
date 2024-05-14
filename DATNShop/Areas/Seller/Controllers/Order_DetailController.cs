using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Seller.Controllers
{
    public class Order_DetailController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Order_Detail
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Add_OrderDetail(string product_name,  int quantity, string color, string size, long order_id)
        {
            var product = new ProductBusiness().searchProduct(product_name);
            var order_detail = db.Order_Detail.Where(x => x.Order_ID == order_id);
            foreach(var item in order_detail)
            {
                if(item.Product_ID == product.ID)
                {
                    item.Quantity += quantity;
                    item.Amount = (int)item.Price * item.Quantity;
                    break;
                }
                else if (item.Product_ID != product.ID)
                {
                    var od = new Order_Detail();
                    od.Product_ID = product.ID;
                    od.Quantity = quantity;
                    od.Order_ID = order_id;
                    if (product.Price != null)
                    {
                        od.Price = product.Price;
                        od.Amount = (int)product.Price * quantity;
                    }
                    else
                    {
                        od.Price = product.Promotion_Price;
                        od.Amount = (int)product.Promotion_Price * quantity;
                    }

                    db.Order_Detail.Add(od);
                    break;
                }

            }
            db.SaveChanges();
            return Json(new
            {
                status = true
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Edit_OrderDetail(long ID, int Quantity)
        {
            var order_detail = db.Order_Detail.Find(ID);
            order_detail.Quantity = Quantity;
            db.SaveChanges();
            return Json(new { 
                status = true 
            });
        }

        public JsonResult Delete_OrderDetail(long ID)
        {
            var order_detail = db.Order_Detail.Find(ID);
            db.Order_Detail.Remove(order_detail);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }


    }
}