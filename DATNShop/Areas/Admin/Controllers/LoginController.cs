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
    public class LoginController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Login
        [CustomRoleProvider(Type = 1)]
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var model = db.Users.Where(x => x.RoleID == 1 && x.Account != "admin").OrderByDescending(x => x.ID).ToPagedList(page, pageSize);
            return View(model);
        }

        //Cập nhật trạng thái
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

        //Cập nhật phân quyền
        public JsonResult UpdateRole(long ID, int Type)
        {
            var user = db.Users.Find(ID);
            user.Type = Type;
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
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

        [HttpPost]
        public ActionResult Add(User entity)
        {
            try
            {
                entity.RoleID = 1;
                db.Users.Add(entity);
                db.SaveChanges();
                TempData["add_success"] = "Thêm truy cập thành công";
                return RedirectToAction("Index");

            }
            catch
            {
                TempData["add_success"] = "Thêm truy cập KHÔNG thành công";
                return RedirectToAction("Index");
            }
        }
    }
}