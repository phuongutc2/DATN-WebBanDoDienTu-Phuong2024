using Rotativa.MVC;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult frmLogin(User model)
        {
            var res = new UserBusiness().checkLogin(model);
            if (res != null)
            {
                if(res.Status == false)
                {
                    TempData["error"] = "Tài khoản của bạn đã bị khóa.";
                    return Redirect("/User/Login");
                }
                else
                {
                    if(res.RoleID == 1)
                    {
                        Session["admin_login"] = res;
                        return Redirect("/Admin/Home/Index");
                    }
                    else if (res.RoleID == 2)
                    {
                        Session["seller_login"] = res;
                        return Redirect("/Seller/Home");
                    }
                    else
                    {
                        Session["user"] = res;
                        if(Session["check_payment"] != null)
                        {
                            return Redirect("/cart/payment");
                        }
                        else
                        {
                            return Redirect("/");
                        }
                        
                    }
                }
                
            }
            else
            {
                TempData["error"] = "Tài khoản hoặc mật khẩu không chính xác.";
                return Redirect("/User/Login");
            }
        }

        //Login gmail
        public JsonResult LoginGmail(string email, string fullname)
        {
           
            var user = new User();
            user.Fullname = fullname.Trim();
            user.Account = email;
            var res = new UserBusiness().CheckAccount(email);
            if (res)//Tồn tại email thì k lưu csdl
            {
                Session["user"] = new UserBusiness().EmailUser(email);
                return Json(new
                {
                    status = true
                });
            }
            else
            {
                var result = new UserBusiness().AddUserEmail(email, fullname);
                if (result)
                {
                    Session["user"] = new UserBusiness().EmailUser(email);
                    return Json(new
                    {
                        status = true
                    });
                }

                else
                    return Json(new
                    {
                        status = false
                    });
            }

        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult frmRegister(User entity)
        {
            var res = new UserBusiness().register(entity);
            if (res)
            {
                TempData["error"] = "Bạn vui lòng đăng nhập để sử dụng dịch vụ của E-Shopper.";
                return RedirectToAction("Login");
            }
            else
            {
                TempData["error"] = "Đã có lỗi xảy ra, vui lòng thử lại sau.";
                return RedirectToAction("Register");
            }
        }

        public ActionResult changPass(long ID)
        {
            return View();
        }

        [HttpPost]
        public ActionResult frmchangePass(long ID, string ex_password, string Password)
        {
            if(new UserBusiness().checkPass(ex_password, ID))
            {
                new UserBusiness().changePass(ID, Password);
                TempData["error"] = "Bạn vui lòng đăng nhập lại sau khi đổi mật khẩu.";
                Session["user"] = null;
                return RedirectToAction("Login");
            }
            else
            {
                TempData["error"] = "Mật khẩu cũ không trùng, vui lòng nhập lại.";
                return Redirect("/doi-mat-khau/" + ID);
            }
            
        }

        //Thông tin user
        public ActionResult InfoUser()
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
                TempData["update"] = "Cập nhật thông tin thành công";
                TempData["type_alert"] = "success";
                Session["user"] = new UserBusiness().findID(entity.ID);
                return RedirectToAction("InfoUser");
            }
            else
            {
                TempData["update"] = "Cập nhật thông tin KHÔNG thành công";
                TempData["type_alert"] = "error";
                return RedirectToAction("InfoUser");
            }
        }

        //Lịch sử đơn hàng
        public ActionResult OrderHistory()
        {
            var user = Session["user"] as User;
            ViewBag.OrderHistory = new UserBusiness().getOrderHistory(user.ID);
            return View();
        }

        //Chi tiết đơn hàng
        public ActionResult OrderDetail(long ID)
        {
            ViewBag.OrderDetail = new UserBusiness().order_detail(ID);
            ViewBag.Order = new UserBusiness().findOrder(ID);
            return View();
        }

        //Đơn hàng của bạn
        public ActionResult MyOrder()
        {
            var user = Session["user"] as User;
            ViewBag.MyOrder = new UserBusiness().MyOrder(user.ID);
            return View();
        }

        //Hủy đơn hàng
        public JsonResult CancerOrder(long ID)
        {
            new UserBusiness().CancerOrder(ID);
            return Json(new
            {
                status = true
            }) ;
        }

        public ActionResult ExportOrder(long ID)
        {
            ViewBag.OrderDetail = new UserBusiness().order_detail(ID);
            ViewBag.Order = new UserBusiness().findOrder(ID);
            return View();
        }

        //In hóa đơn
        //In hóa đơn
        public ActionResult PrintBill(long ID)
        {
            var report = new ActionAsPdf("ExportOrder", new { ID = ID });
            return report;
        }

        public ActionResult logout()
        {
            Session["user"] = null;
            return Redirect("/");
        }
    }
}