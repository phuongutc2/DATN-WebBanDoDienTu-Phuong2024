using Newtonsoft.Json;
using PagedList;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingOnline.Areas.Seller.Controllers
{
    public class ProductController : Controller
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        // GET: Admin/Product
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;
            if (seller == null)
                return Redirect("/User/Login");
            var query = from pro in db.Products
                        join user in db.Users on pro.User_ID equals user.ID
                        where pro.User_ID == seller.ID && pro.Category.Type == "Laptop"
                        select new ProductDTO()
                        {
                            ID = pro.ID,
                            Product_Name = pro.Product_Name,
                            Product_Code = pro.Product_Code,
                            Promotion_Price = pro.Promotion_Price,
                            Price = pro.Price,
                            Image = pro.Image,
                            Quantity = pro.Quantity,
                            Seller_Name = user.Fullname,
                            Category_Name = pro.Category.Name,
                            Status = pro.Status
                        };
            return View(query.OrderByDescending(x => x.ID).ToPagedList(page, pageSize));
        }

        public ActionResult Phone(int page = 1, int pageSize = 10)
        {
            var seller = Session["seller_login"] as User;
            if (seller == null)
                return Redirect("/User/Login");
            var query = from pro in db.Products
                        join user in db.Users on pro.User_ID equals user.ID
                        where pro.User_ID == seller.ID && pro.Category.Type == "Điện thoại"
                        select new ProductDTO()
                        {
                            ID = pro.ID,
                            Product_Name = pro.Product_Name,
                            Product_Code = pro.Product_Code,
                            Promotion_Price = pro.Promotion_Price,
                            Price = pro.Price,
                            Image = pro.Image,
                            Quantity = pro.Quantity,
                            Seller_Name = user.Fullname,
                            Status = pro.Status
                        };
            return View(query.OrderByDescending(x => x.ID).ToPagedList(page, pageSize));
        }

        // GET: Admin/Product/Create
        public ActionResult Add()
        {
            ViewBag.lstCategory = db.Categories.ToList();
            return View();
        }

        // POST: Admin/Product/Create
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Product entity, HttpPostedFileBase Image)
        {
            try
            {
                long maxid = db.Products.Max(x => x.ID);
                var pro = new Product();
                pro.Product_Name = entity.Product_Name;
                pro.Product_Code = "PRO" + (maxid + 1).ToString() + DateTime.Now.ToString("ddMMyyyy");
                pro.Metatitle = Str_Metatitle(entity.Product_Name);
                pro.Promotion_Price = entity.Promotion_Price;
                pro.Price = entity.Price;
                pro.User_ID = entity.User_ID;
                pro.Category_ID = entity.Category_ID;
                pro.Desription = entity.Desription;
                pro.Configuration = entity.Configuration;
                pro.Quantity = 0;
                pro.Status = true;
               
                //Thêm hình ảnh
                var path = Path.Combine(Server.MapPath("~/Assets/images/product"), Image.FileName);
                if (System.IO.File.Exists(path))
                {
                    string extensionName = Path.GetExtension(Image.FileName);
                    string filename = Image.FileName +DateTime.Now.ToString("ddMMyyyy") + extensionName;
                    path = Path.Combine(Server.MapPath("~/Assets/images/product/"), filename);
                    Image.SaveAs(path);
                    pro.Image = filename;
                }
                else
                {
                    Image.SaveAs(path);
                    pro.Image = Image.FileName;
                }

                db.Products.Add(pro);
                db.SaveChanges();
                TempData["add_success"] = "Thêm sản phẩm thành công.";

                if(db.Categories.Find(entity.Category_ID).Type == "Laptop")
                    return RedirectToAction("Index");
                else
                    return RedirectToAction("Phone");
            }
            catch
            {
                return RedirectToAction("Add");
            }
        }

        // GET: Admin/Product/Edit/5
        public ActionResult Edit(long ID)
        {
            ViewBag.product = db.Products.Find(ID);
            ViewBag.lstCategory = db.Categories.ToList();
            return View();
        }

        // POST: Admin/Product/Edit/5
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(Product entity, HttpPostedFileBase Image)
        {
            try
            {
                var pro = db.Products.Find(entity.ID);
                pro.Product_Name = entity.Product_Name;
                pro.Metatitle = Str_Metatitle(entity.Product_Name);
                pro.Promotion_Price = entity.Promotion_Price;
                pro.Price = entity.Price;
                pro.User_ID = entity.User_ID;
                pro.Category_ID = entity.Category_ID;
                pro.Desription = entity.Desription;
                pro.Configuration = entity.Configuration;
                try
                {
                    if (pro.Image != Image.FileName)
                    {
                        //Xóa file cũ
                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/Assets/images/product"), pro.Image));
                        //Thêm hình ảnh
                        var path = Path.Combine(Server.MapPath("~/Assets/images/product"), Image.FileName);
                        if (System.IO.File.Exists(path))
                        {
                            string extensionName = Path.GetExtension(Image.FileName);
                            string filename = Image.FileName + DateTime.Now.ToString("ddMMyyyy") + extensionName;
                            path = Path.Combine(Server.MapPath("~/Assets/images/product/"), filename);
                            Image.SaveAs(path);
                            pro.Image = filename;
                        }
                        else
                        {
                            Image.SaveAs(path);
                            pro.Image = Image.FileName;
                        }
                    }
                    db.SaveChanges();
                    TempData["add_success"] = "Cập nhật sản phẩm thành công.";
                    if (db.Categories.Find(entity.Category_ID).Type == "Laptop")
                        return RedirectToAction("Index");
                    else
                        return RedirectToAction("Phone");
                }
                catch
                {
                    db.SaveChanges();
                    TempData["add_success"] = "Cập nhật sản phẩm thành công.";
                    return RedirectToAction("Index");
                }
                
                

                
            }
            catch
            {
                return View();
            }
        }

        // GET: Admin/Product/Delete/5
        public JsonResult Delete(long ID)
        {
            try
            {
                var product = db.Products.Find(ID);
                var lstImage = db.Images.Where(x => x.Product_ID == ID).ToList();
                foreach(var item in lstImage)
                {
                    System.IO.File.Delete(Path.Combine(Server.MapPath("~/Assets/images/product-detail"), item.Image1));
                    db.Images.Remove(item);
                }
                System.IO.File.Delete(Path.Combine(Server.MapPath("~/Assets/images/product"), product.Image));

                db.Products.Remove(product);
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

        public JsonResult changeStatus(long ID)
        {
            var pro = db.Products.Find(ID);
            if (pro.Status == true)
                pro.Status = false;
            else
                pro.Status = true;
            db.SaveChanges();
            return Json(new
            {
                status = true
            });
        }

        public JsonResult GetCategoryByType(string type)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var model = db.Categories.Where(x => x.Type == type).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Images(long ID)
        {
            ViewBag.lstImage = db.Images.Where(x => x.Product_ID == ID).OrderBy(x => x.ID).ToList();
            ViewBag.Product = db.Products.Find(ID);
            return View();
        }


        [HttpPost]
        public ActionResult Upload_Mutil_Image(long Product_ID, HttpPostedFileBase[] images)
        {
            //Ensure model state is valid  
            if (ModelState.IsValid)
            {   //iterating through multiple file collection   
                foreach (HttpPostedFileBase file in images)
                {
                    var imgs = new Image();
                    imgs.Product_ID = Product_ID;
                    var path = Path.Combine(Server.MapPath("~/Assets/images/product-detail"), file.FileName);
                    if (System.IO.File.Exists(path))
                    {
                        string extensionName = Path.GetExtension(file.FileName);
                        string filename = file.FileName + DateTime.Now.ToString("ddMMyyyy") + extensionName;
                        path = Path.Combine(Server.MapPath("~/Assets/images/product-detail"), filename);
                        file.SaveAs(path);
                        imgs.Image1 = filename;
                        db.Images.Add(imgs);
                        db.SaveChanges();

                    }
                    else
                    {
                        file.SaveAs(path);
                        imgs.Image1 = file.FileName;
                        db.Images.Add(imgs);
                        db.SaveChanges();
                    }

                }
            }
            TempData["add_success"] = "Thêm hình ảnh thành công.";
            TempData["alert"] = "alert-success";
            return RedirectToAction("Images", new { ID = Product_ID });
        }

        public JsonResult Del_Images(long ID)
        {

            var img = db.Images.Find(ID);
            System.IO.File.Delete(Path.Combine(Server.MapPath("~/Assets/images/product-detail"), img.Image1));
            db.Images.Remove(img);
            db.SaveChanges();
            return Json(new
            {
                status = true
            });

        }

        //Chuyển tên sản phẩm thành metatitle
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
                {
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]).Replace("“", string.Empty).Replace("”", string.Empty);
                    str = str.Replace("\"", string.Empty).Replace("'", string.Empty).Replace("`", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty);
                    str = str.Replace(".", string.Empty).Replace(",", string.Empty).Replace(";", string.Empty).Replace(":", string.Empty);
                    str = str.Replace("?", string.Empty);
                }
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
