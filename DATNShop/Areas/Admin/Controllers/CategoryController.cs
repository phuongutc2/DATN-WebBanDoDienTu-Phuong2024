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
    public class CategoryController : Controller
    {
        // GET: Admin/Category
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Provider
        [CustomRoleProvider(Type = 1)]
        public ActionResult Index()
        {
            ViewBag.laptop = db.Categories.Where(x => x.Type == "Laptop").OrderByDescending(x => x.ID).ToList();
            ViewBag.phone = db.Categories.Where(x => x.Type == "Điện thoại").OrderByDescending(x => x.ID).ToList();
            return View();
        }


        //Xóa tài khoản
        public JsonResult Delete(long ID)
        {

            try
            {
                var category = db.Categories.Find(ID);
                db.Categories.Remove(category);
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


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult addCategory(Category entity)
        {
            try
            {
                var maxid = db.Categories.Max(x => x.ID);
                var prv = new Category();
                prv.Code = "VENDOR" + (maxid + 1).ToString() + DateTime.Now.ToString("ddMMyyyy");
                prv.Name = entity.Name.Trim();
                prv.Type = entity.Type;
                prv.Metatitle = Str_Metatitle(entity.Name.Trim());
                db.Categories.Add(prv);
                db.SaveChanges();
                TempData["add_success"] = "Thêm danh mục sản phẩm thành công";
                return RedirectToAction("Index");

            }
            catch
            {
                TempData["add_success"] = "Thêm danh mục sản phẩm KHÔNG thành công";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult editCategory(Category entity)
        {
            try
            {
                var prv = db.Categories.Find(entity.ID);
                prv.Name = entity.Name.Trim();
                prv.Type = entity.Type;
                prv.Metatitle = Str_Metatitle(entity.Name.Trim());
                db.SaveChanges();
                TempData["add_success"] = "Cập nhật danh mục sản phẩm thành công";
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["add_success"] = "Cập nhật danh mục sản phẩm KHÔNG thành công";
                return RedirectToAction("Index");
            }
        }

        public JsonResult GetCategoryByID(long ID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var category = db.Categories.Find(ID);
            return Json(category, JsonRequestBehavior.AllowGet);
        }


        public string Str_Metatitle(string str)
        {
            string[] VietNamChar = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ:/"
            };
            //Thay thế và lọc dấu từng char      
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
            }
            string str1 = str.ToLower();
            string[] name = str1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string meta = null;
            //Thêm dấu '-'
            foreach (var item in name)
            {
                meta = meta + item + "-";
            }
            return meta;
        }
    }
}