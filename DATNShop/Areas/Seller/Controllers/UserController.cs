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
    public class UserController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/User
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;

            var lstOrder = db.Orders.Where(x => x.Seller_ID == seller.ID).ToList();
            var model = new List<UserDTO>();
            foreach(var item in db.Users.ToList())
            {
                if(lstOrder.Exists(x => x.User_ID == item.ID))
                {
                    var user = new UserDTO();
                    user.Fullname = item.Fullname;
                    user.Address = item.Address;
                    user.Phone = item.Phone;
                    user.Email = item.Email;
                    
                    user.TotalMoney = 0;
                    user.TotalQuantity = 0;
                    foreach(var jtem in lstOrder.Where(x => x.User_ID == item.ID).OrderBy(x => x.CreatedDate).ToList())
                    {
                        user.TotalMoney += jtem.TotalMoney;
                        user.TotalQuantity += jtem.TotalQuantity;
                        user.LastDayBought = jtem.CreatedDate;
                    }

                    model.Add(user);
                }
            }
                    
            return View(model.OrderByDescending(x => x.TotalMoney).ToPagedList(page, pageSize));
        }

        //Phân quyền
        
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SellerLogin(User model)
        {
            var result = db.Users.Where(x => x.Account == model.Account && x.Password == model.Password && x.RoleID == 2).ToList();
            if (result.Count > 0)
            {
                Session["seller_login"] = db.Users.SingleOrDefault(x => x.Account == model.Account && x.Password == model.Password && x.RoleID == 2);
                return Redirect("/Seller/Home");
            }
            else
            {
                TempData["error_login"] = "Tài khoản hoặc mật khẩu không chính xác";
                return RedirectToAction("Login");
            }
        }

        public ActionResult changPass()
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
                return Redirect("/Seller/User/changPass");
            }

        }

        public ActionResult Update()
        {
            return View();
        }

        //cập nhật thông tin user
        [HttpPost]
        public ActionResult UpdateUser(User entity)
        {
            var res = new UserBusiness().UpdateUser(entity);
            if (res)
            {
                TempData["message"] = "Cập nhật thông tin thành công";
                TempData["alert"] = "alert-success";
                Session["seller_login"] = db.Users.Find(entity.ID);
                return RedirectToAction("Update");
            }
            else
            {
                TempData["message"] = "Cập nhật thông tin KHÔNG thành công";
                TempData["alert"] = "alert-danger";
                return RedirectToAction("Update");
            }
        }

        public ActionResult Logout()
        {
            Session["seller_login"] = null;
            return Redirect("/User/Login");
        }
        
    }
}