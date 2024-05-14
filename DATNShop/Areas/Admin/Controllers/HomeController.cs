using Rotativa.MVC;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Home
        public ActionResult Index()
        {
            if (Session["admin_login"] == null)
                return Redirect("/User/Login");
            //thống kê sp bán ra
            var sell = from detail in db.Order_Detail
                       join order in db.Orders on detail.Order_ID equals order.ID
                       where order.Status == 3 || order.Payment == 1
                       select new
                       {
                           detail.Quantity,
                           detail.Amount
                       };
            ViewBag.Count_sell = db.Orders.Where(x =>  x.Payment == 1 ||  x.Status == 3).Sum(x => x.TotalQuantity);

            //Thống kê doanh thu
            ViewBag.Count_money = db.Orders.Where(x =>  x.Payment == 1 ||  x.Status == 3).Sum(x => x.TotalMoney);

            //Doanh thu hôm nay
            ViewBag.Money_today = db.Orders.Where(x =>  DbFunctions.TruncateTime(x.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) &&
                                                        x.Payment == 1 || 
                                                        DbFunctions.TruncateTime(x.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now) &&
                                                        x.Status == 3).Select(x => x.TotalMoney).Sum();
            //Thống kê đơn đặt hàng
            ViewBag.Count_Order = db.Orders.Count();


            //Thống kê tồn ko
            ViewBag.quantity_product = db.Products.Where(x => x.Quantity > 0).OrderByDescending(x => x.Quantity).Take(10).ToList();

            //Thống kê lượng hàng bán ra

            var listProduct_sell = new List<Order_DetailDTO>();
            foreach (var item in db.Order_Detail.Where(x => x.Order.Payment == 1 || x.Order.Status == 3).ToList())
            {

                if (listProduct_sell.Exists(x => x.Product_ID == item.Product_ID))
                {
                    foreach (var jtem in listProduct_sell)
                    {
                        if (jtem.Product_ID == item.Product_ID)
                        {
                            jtem.Quantity += item.Quantity;
                            jtem.Amount += item.Amount;
                        }
                    }

                }
                else
                {
                    var productsell = new Order_DetailDTO();
                    productsell.Product_Name = item.Product.Product_Name;
                    productsell.Product_ID = item.Product_ID;
                    productsell.Quantity = item.Quantity;
                    productsell.Amount = item.Amount;
                    listProduct_sell.Add(productsell);
                }
            }

            ViewBag.product_sell = listProduct_sell.OrderByDescending(x => x.Quantity).Take(10).ToList();

            //Thống kê đánh giá hôm nay
            var Review_today = from review in db.Reviews
                               join pro in db.Products on review.Product_ID equals pro.ID
                               select new ReviewDTO()
                               {
                                   ID = review.ID,
                                   CreatedDate = review.CreatedDate
                               };
            ViewBag.Review_today = Review_today.Where(x => DbFunctions.TruncateTime(x.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now)).Count();

            //Thống kê đơn đạt hàng hôm nay
            var Order_today = from order in db.Orders
                              join user in db.Users on order.Seller_ID equals user.ID
                              select new
                              {
                                  order.ID,
                                  order.Payment,
                                  order.Status,
                                  order.CreatedDate
                              };
            ViewBag.Order_today = Order_today.Where(x => DbFunctions.TruncateTime(x.CreatedDate) == DbFunctions.TruncateTime(DateTime.Now)).Count();

            //Đơn hàng đã thanh toán
            ViewBag.Order_Payment = Order_today.Where(x => x.Payment == 1 || x.Status == 3).Count();

            //Đơn hàng đang giao
            ViewBag.Ordering = Order_today.Where(x => x.Status == 2).Count();

            //Đơn hàng đã hủy
            ViewBag.Order_Cancer = Order_today.Where(x => x.Status == 0).Count();

            //Đơn hàng đã từ chối nhận
            ViewBag.Order_refuse = Order_today.Where(x => x.Status == -1).Count();

            return View();
        }

        public ActionResult Export()
        {
            var lstproduct_id = db.Order_Detail.Select(x => x.Product_ID).Distinct();
            var listProduct_sell = new List<Order_DetailDTO>();
            foreach (var item in lstproduct_id)
            {
                var product = db.Products.Find(item);
                var productsell = new Order_DetailDTO();
                productsell.Product_Name = product.Product_Name;
                productsell.Quantity = 0;
                productsell.Amount = 0;
                foreach (var jtem in db.Order_Detail.Where(x => x.Product_ID == item))
                {
                    productsell.Quantity += jtem.Quantity;
                    productsell.Amount += jtem.Amount;
                }
                listProduct_sell.Add(productsell);
            }
            ViewBag.product_sell = listProduct_sell.OrderByDescending(x => x.Quantity).Take(10).ToList();
            return View();
        }

        //In hóa đơn
        //In hóa đơn
        public ActionResult PrintBill()
        {
            var report = new ActionAsPdf("Export");
            return report;
        }

        [ChildActionOnly]
        public PartialViewResult MainMenu()
        {
            ViewBag.lstCategory = new ProductBusiness().GetCategories();
            return PartialView();
        }

        public ActionResult TotalSaleMonth()
        {
            int[] month = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            var lstTotal = new List<TotalSale>();
            for (int i = 1; i <= 12; i++)
            {
                var model = db.Orders.Where(x => 
                                                x.CreatedDate.Value.Month == i &&
                                                x.Payment == 1 ||
                                                x.CreatedDate.Value.Month == i &&
                                                x.Status == 3).Sum(x => x.TotalMoney);

                var totalsale = new TotalSale();
                totalsale.thang = i;
                if (model != null)
                    totalsale.tong = model;
                else
                    totalsale.tong = 0;
                lstTotal.Add(totalsale);
            }
            return Json(lstTotal, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CateSale_Laptop()
        {
            var lstTotal = new List<TotalSale>();
            foreach (var item in db.Categories.Where(x => x.Type == "Laptop").ToList())
            {
                var model = (from or in db.Orders
                             join detail in db.Order_Detail on or.ID equals detail.Order_ID
                             join pro in db.Products on detail.Product_ID equals pro.ID
                             where pro.Category_ID == item.ID && or.Payment == 1 || pro.Category_ID == item.ID && or.Status == 3
                             select new TotalSale
                             {
                                 tong = or.TotalMoney
                             }).Sum(x => x.tong);
                var totalsale = new TotalSale();
                totalsale.CategoryName = item.Name;
                if (model != null)
                    totalsale.tong = model;
                else
                    totalsale.tong = 0;
                lstTotal.Add(totalsale);
            }
            return Json(lstTotal, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CateSale_Phone()
        {
            var lstTotal = new List<TotalSale>();
            foreach (var item in db.Categories.Where(x => x.Type == "Điện thoại").ToList())
            {
                var model = (from or in db.Orders
                             join detail in db.Order_Detail on or.ID equals detail.Order_ID
                             join pro in db.Products on detail.Product_ID equals pro.ID
                             where pro.Category_ID == item.ID && or.Payment == 1 || pro.Category_ID == item.ID && or.Status == 3
                             select new TotalSale
                             {
                                 tong = or.TotalMoney
                             }).Sum(x => x.tong);
                var totalsale = new TotalSale();
                totalsale.CategoryName = item.Name;
                if (model != null)
                    totalsale.tong = model;
                else
                    totalsale.tong = 0;
                lstTotal.Add(totalsale);
            }
            return Json(lstTotal, JsonRequestBehavior.AllowGet);

        }
    }
}