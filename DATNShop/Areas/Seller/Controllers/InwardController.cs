using PagedList;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Seller.Controllers
{
    public class InwardController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Seller/Inward
        public ActionResult Index(int page = 1, int pagesize = 12)
        {
            var seller = Session["seller_login"] as User;
            if (seller == null)
                return Redirect("/User/Login");
            var model = db.Inwards.Where(x => x.User_ID == seller.ID).OrderByDescending(x => x.Createdate).ToPagedList(page, pagesize);
            //Thống kê tồn ko
            ViewBag.quantity_product = db.Products.Where(x => x.User_ID == seller.ID && x.Quantity > 0).OrderByDescending(x => x.Quantity).Take(12).ToList();
            return View(model);
        }

        public ActionResult Add()
        {
            var seller = Session["seller_login"] as User;
            if (seller == null)
                return Redirect("/User/Login");
            ViewBag.lstproduct = db.Products.Where(x => x.User_ID == seller.ID).OrderByDescending(x => x.Product_Name).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddInward(Inward entity)
        {
            var res = new InwardBusiness().addInward(entity);
            if (res)
            {
                var cart = (List<CartDTO>)Session["add_inward"];
                foreach (var item in cart)
                {
                    var detail = new Inward_Detail();
                    detail.Inward_ID = db.Inwards.Max(x => x.ID);
                    detail.Product_ID = item.Product.ID;
                    detail.Quantity = item.Quantity;

                    if (item.Product.Price != null)
                    {
                        detail.Price = item.Product.Price;
                        detail.Amount = (int)item.Product.Price * item.Quantity;
                    }
                    else
                    {
                        detail.Price = item.Product.Promotion_Price;
                        detail.Amount = (int)item.Product.Promotion_Price * item.Quantity;
                    }
                    new InwardBusiness().addInward_Detail(detail);

                    //Cộng tồn kho
                    new ProductBusiness().AddQuantity(item.Product.ID, item.Quantity);
                }
                Session["add_order"] = null;
                TempData["add_success"] = "Nhập kho thành công.";
                TempData["alert"] = "alert-success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["alert"] = "alert-danger";
                TempData["add_success"] = "Nhập kho KHÔNG thành công.";
                return RedirectToAction("Add");
            }
        }


        public JsonResult addInwardProduct(long product_id, int quantity)
        {
            var product = db.Products.Find(product_id);
            var cart = Session["add_inward"];
            if (cart != null)//Nếu giỏ đã chứa sản phẩm
            {
                var list = (List<CartDTO>)cart;
                if (list.Exists(x => x.Product.ID == product.ID))
                {
                    foreach (var item in list)
                    {
                        if (item.Product.ID == product.ID)
                        {
                            item.Quantity += quantity;
                        }
                    }
                }
                else
                {
                    //Tạo đối tượng mới
                    var item = new CartDTO();
                    item.Product = product;
                    item.Quantity = quantity;
                    list.Add(item);
                }
            }
            else//nếu giỏ hàng trống
            {
                var item = new CartDTO();
                item.Product = product;
                item.Quantity = quantity;
                var list = new List<CartDTO>();
                list.Add(item);

                Session["add_inward"] = list;
            }
            return Json(new
            {
                status = true
            }, JsonRequestBehavior.AllowGet);

        }


        //Xóa từng sản phẩm
        public JsonResult Delete_InwardProduct(long ID)
        {
            var cartSec = (List<CartDTO>)Session["add_inward"];
            cartSec.RemoveAll(x => x.Product.ID == ID);
            Session["add_inward"] = cartSec;
            return Json(new
            {
                status = true
            });
        }

        //Sửa số lượng sp trong giỏ hàng
        public JsonResult Edit(long ID, int Quantity)
        {
            var productSec = (List<CartDTO>)Session["add_inward"];

            foreach (var item in productSec)
            {
                if (item.Product.ID == ID)
                {
                    item.Quantity = Quantity;
                }

            }

            Session["add_inward"] = productSec;
            return Json(new
            {
                status = true
            });
        }


        //Chi tiết nhập kho
        public ActionResult Inward_Detail(long ID)
        {
            var model = db.Inward_Detail.Where(x => x.Inward_ID == ID);
            ViewBag.Inward = db.Inwards.Find(ID);
            return View(model.ToList());
        }
    }
}