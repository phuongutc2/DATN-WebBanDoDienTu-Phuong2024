using PagedList;
using ShoppingOnline.Models.Business;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace ShoppingOnline.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(long ID)
        {
            var model = new ProductBusiness();
            var product = model.getProductDetail(ID);
            ViewBag.product_detail = product;
            ViewBag.lstProduct_Recommend = model.GetProducts_Recomment(ID);
            ViewBag.lstProductRecommend_1 = model.getProductRecommend();
            ViewBag.lstProductRecommend_2 = model.getProductRecommend();
            ViewBag.lstCategory = model.GetCategories();
            ViewBag.lstSameCategory = model.getProductByCategoryID(product.Category_ID);
            ViewBag.lstReview = model.getReview(ID);
            ViewBag.lstImage = model.getLstImage(ID);
            return View();
        }

        public JsonResult addReview(string json_review)
        {
            var JsonReview = new JavaScriptSerializer().Deserialize<List<ReviewDTO>>(json_review);
            var review = new Review();
            foreach (var item in JsonReview)
            {
                review.Content = item.Content;
                review.Rating = item.Rating;
                review.CreatedDate = DateTime.Now;
                review.User_ID = item.User_ID;
                review.Product_ID = item.Product_ID;
                review.Status = true;
            }
            
            var res = new ProductBusiness().addReview(review);
            if (res)
            {
                return Json(new
                {
                    status = true
                });
            }
            else
            {
                return Json(new
                {
                    status = false
                });
            }
        }

        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        public JsonResult ListName(string q)
        {
            var query = db.Products.Where(x => x.Product_Name.Contains(q)).Select(x => new { x.Product_Name, x.Image });
            return Json(new
            {
                data = query,
                status = true
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search(string keyword, string order = null, int page = 1, int pagesize = 16)
        {
            string[] key = keyword.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var product_name = new List<Product>();//Tìm theo tên sản phẩm
            foreach (var item in key)
            {
                if(order == "asc")
                {
                    product_name = (from b in db.Products
                                    where b.Product_Name.Contains(item)
                                    select b).OrderBy(x => x.Promotion_Price).ToList();
                }
                else if(order == "desc")
                {
                    product_name = (from b in db.Products
                                    where b.Product_Name.Contains(item)
                                    select b).OrderByDescending(x => x.Promotion_Price).ToList();
                }
                else
                {
                    product_name = (from b in db.Products
                                    where b.Product_Name.Contains(item)
                                    select b).ToList();
                    if (db.Products.Where(x => x.Product_Name == keyword).Count() > 0)
                    {
                        product_name.Remove(db.Products.Where(x => x.Product_Name == keyword).First());
                        product_name.Insert(0, db.Products.Where(x => x.Product_Name == keyword).First());
                    }
                       
                }
                
            }
            ViewBag.KeyWord = keyword;
            ViewBag.lstCategory = new ProductBusiness().GetCategories();
            ViewBag.orderby = order;
            return View(product_name.ToPagedList(page, pagesize));
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
                meta = meta + item + " ";
            }
            return meta;
        }

    }
}