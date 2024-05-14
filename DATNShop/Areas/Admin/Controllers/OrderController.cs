using PagedList;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Order
        //Đơn đã thanh toán
        public ActionResult Index(int page = 1, int pagesize = 10)
        {
            var model = db.Orders.Where(x => x.Status == 3 || x.Payment == 1).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pagesize);
            return View(model);
        }

        //Đơn chờ xác nhận
        public ActionResult WaitAccept(int page = 1, int pagesize = 10)
        {
            var model = db.Orders.Where(x => x.Status == 1).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pagesize);
            return View(model);
        }

        //Đơn bị hủy
        public ActionResult CancerOrder(int page = 1, int pagesize = 10)
        {
            var model = db.Orders.Where(x => x.Status == 0 || x.Status == -1).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pagesize);
            return View(model);
        }

        //Đơn đang vận chuyển
        public ActionResult OrderDelivering(int page = 1, int pagesize = 10)
        {
            var model = db.Orders.Where(x => x.Status == 2).OrderByDescending(x => x.CreatedDate).ToPagedList(page, pagesize);
            return View(model);
        }

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

    }
}