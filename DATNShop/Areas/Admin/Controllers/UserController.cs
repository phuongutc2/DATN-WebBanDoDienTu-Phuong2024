using PagedList;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Admin.Controllers
{
    public class UserController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/User
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var lstOrder = db.Orders.ToList();
            var model = new List<UserDTO>();
            foreach (var item in db.Users.ToList())
            {
                
                    var user = new UserDTO();
                    user.Fullname = item.Fullname;
                    user.Address = item.Address;
                    user.Phone = item.Phone;
                    user.Email = item.Email;
                    user.RoleID = item.RoleID;
                    user.User_ID = item.ID;
                    user.Status = item.Status;
                    
                    user.TotalMoney = 0;
                    user.TotalQuantity = 0;
                    foreach (var jtem in lstOrder.Where(x => x.User_ID == item.ID).OrderBy(x => x.CreatedDate).ToList())
                    {
                        user.TotalMoney += jtem.TotalMoney;
                        user.TotalQuantity += jtem.TotalQuantity;
                        user.LastDayBought = jtem.CreatedDate;
                    }

                    model.Add(user);
            }
            return View(model.Where(x => x.RoleID == 3).OrderByDescending(x => x.TotalMoney).ToPagedList(page, pageSize));
        }

        public ActionResult Seller(int page = 1, int pageSize = 10)
        {
            var lstOrder = db.Orders.ToList();
            var model = new List<UserDTO>();
            foreach (var item in db.Users.Where(x => x.RoleID == 2).ToList())
            {

                var user = new UserDTO();
                user.Fullname = item.Fullname;
                user.Address = item.Address;
                user.Phone = item.Phone;
                user.Email = item.Email;
                user.RoleID = item.RoleID;
                user.User_ID = item.ID;
                user.Status = item.Status;

                user.TotalMoney = 0;
                user.TotalQuantity = 0;
                user.Quantity = db.Products.Where(x => x.User_ID == item.ID).Count();
                foreach (var jtem in lstOrder.Where(x => x.Seller_ID == item.ID).ToList())
                {
                    user.TotalMoney += jtem.TotalMoney;
                    user.TotalQuantity += jtem.TotalQuantity;
                }

                model.Add(user);
            }
            return View(model.Where(x => x.RoleID == 2).OrderByDescending(x => x.TotalQuantity).ToPagedList(page, pageSize));
        }

        //Phân quyền
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult adminLogin(User model)
        {
            var result = db.Users.Where(x => x.Account == model.Account && x.Password == model.Password && x.Type == 1 ).ToList();
            if(result.Count > 0)
            {
                Session["admin_login"] = db.Users.SingleOrDefault(x => x.Account == model.Account && x.Password == model.Password && x.Type == 1);
                return Redirect("/Admin/Home/Index");
            }
            else
            {
                TempData["error_login"] = "Tài khoản hoặc mật khẩu không chính xác";
                return RedirectToAction("Login");
            }
        }

        public ActionResult Logout()
        {
            Session["admin_login"] = null;
            return Redirect("/User/Login");
        }

        //Xóa tài khoản
        public JsonResult Delete(long ID)
        {
            var user = db.Users.Find(ID);
            db.Users.Remove(user);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        public JsonResult changeStatus(long ID)
        {
            var user = db.Users.Find(ID);
            if (user.Status == true)
                user.Status = false;
            else
                user.Status = true;
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        //public JsonResult ListName(string q)
        //{
        //    var query = db.Users.Where(x => x.Fullname.Contains(q)).Select(x => x.Fullname);
        //    //var query = from pro in db.Products
        //    //            where pro.Product_Name.Contains(q)
        //    //            select new
        //    //            {
        //    //                pro.Product_Name,
        //    //                pro.Image
        //    //            };
        //    return Json(new
        //    {
        //        data = query,
        //        status = true
        //    }, JsonRequestBehavior.AllowGet);
        //}

        //public ActionResult Search(string keyword, int page = 1, int pagesize = 6)
        //{
        //    string[] key = keyword.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

        //    var user_name = new List<User>();//Tìm theo tên sản phẩm
        //    foreach (var item in key)
        //    {
        //        user_name = (from b in db.Users
        //                        where b.Fullname.Contains(item)
        //                        select b).ToList();
        //    }
        //    ViewBag.KeyWord = keyword;
        //    return View(user_name.ToPagedList(page, pagesize));
        //}

        public ActionResult ChangePass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult frmchangePass(long ID, string ex_password, string Password)
        {
            if (new UserBusiness().checkPass(ex_password, ID))
            {
                new UserBusiness().changePass(ID, Password);
                Session["seller_login"] = null;
                TempData["error"] = "Bạn vui lòng đăng nhập lại sau khi đổi mật khẩu.";
                return Redirect("/User/Login");
            }
            else
            {
                TempData["error"] = "Mật khẩu cũ không đúng, vui lòng nhập lại.";
                return Redirect("/Admin/User/ChangePass");
            }

        }
    }
}