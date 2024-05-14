using PagedList;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Seller.Controllers
{
    public class OrderController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Order
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;
            var model = db.Orders.Where(x => x.Seller_ID == seller.ID && x.Status == 1 ).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pageSize);
            return View(model);
        }

        public ActionResult Delivery(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;
            var model = db.Orders.Where(x => x.Seller_ID == seller.ID && x.Status == 2).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pageSize);
            return View(model);
        }

        public ActionResult Payment(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;
            var model = db.Orders.Where(x => x.Seller_ID == seller.ID && x.Status == 3 || x.Seller_ID == seller.ID && x.Payment == 1).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pageSize);
            return View(model);
        }
        
        public ActionResult Cancer(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;
            var model = db.Orders.Where(x => x.Seller_ID == seller.ID && x.Status == -1 || x.Seller_ID == seller.ID && x.Status == 0).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pageSize);
            return View(model);
        }

        //Xóa đơn hàng
        public JsonResult Delete(long ID)
        {
            try
            {
                var order = db.Orders.Find(ID);
                order.Status = -1;
                order.CancerDate = DateTime.Now;
                db.SaveChanges();
                return Json(new
                {
                    status = true
                });
            }
            catch
            {
                return Json(new
                {
                    status = false
                });
            }
            
        }

        //kích hoạt đã thanh toán
        public JsonResult changStatus(long ID)
        {
            var order = db.Orders.Find(ID);
            if (order.Status == 1)
            {
                order.Status = 2;
                order.ShipDate = DateTime.Now;
            }
            else if (order.Status == 2)
                order.Status = 3;

            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        //Chi tiết đơn hàng
        public ActionResult Order_Detail(long ID)
        {
            var query = from order_detail in db.Order_Detail
                        join pro in db.Products on order_detail.Product_ID equals pro.ID
                        where order_detail.Order_ID == ID
                        select new Order_DetailDTO()
                        {
                            ID = order_detail.ID,
                            Product_Name = pro.Product_Name,
                            Quantity = order_detail.Quantity,
                            Price = order_detail.Price,
                            Amount = order_detail.Amount
                        };
            ViewBag.order = db.Orders.Find(ID);
            return View(query.OrderByDescending(x => x.ID).ToList());
        }

        //lấy serial_type
        //public JsonResult getSerial(string product_name)
        //{
        //    db.Configuration.ProxyCreationEnabled = false;
        //    var product = new ProductBusiness().searchProduct(product_name);
        //    //var query = db.Serial_Type.Where(x => x.Product_ID == product.ID).ToList();
        //    return Json(query, JsonRequestBehavior.AllowGet);
        //}

       
    }
}