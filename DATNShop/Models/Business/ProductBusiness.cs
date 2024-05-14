using PagedList;
using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.Business
{
    public class ProductBusiness
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();

        public Product findID(long ID)
        {
            return db.Products.Find(ID);
        }

        //Tìm kiếm sản phẩm theo tên sp
        public Product searchProduct(string product_name)
        {
            return db.Products.Single(x => x.Product_Name == product_name);
        }
        
        //Lấy chi tiết hình ảnh sản phẩm
        public List<Image> getLstImage(long ID)
        {
            return db.Images.Where(x => x.Product_ID == ID).ToList();
        }

        //Cộng tồn kho
        public void AddQuantity(long ID, int quantity)
        {
            var product = db.Products.Find(ID);
            product.Quantity += quantity;
            db.SaveChanges();
        }

        //Trừ tồn kho sau khi thanh toán đơn hàng
        public void Subtract_Quantity(long ID, int quantity)
        {
            var product = db.Products.Find(ID);
            product.Quantity -= quantity;
            db.SaveChanges();
        }

        //Lấy ngẫu nhiên 6 sản phẩm
        public List<Product> getAll()
        {
            var lst_id = db.Products.Max(x => x.ID);
            
            var lst_pro = new List<Product>();
            int dem = 0;
            var random = new Random();
            while (true)
            {
                if (dem == 16)
                    break;
                long index = random.Next((int)lst_id);
                var product = db.Products.Find(index);
                if(product != null)
                {
                    lst_pro.Add(product);
                    dem++;
                }
                   
            }

            return lst_pro;   
        }

        //Lấy toàn bộ sản phẩm
        public List<Product> getAllProduct()
        {
            return db.Products.ToList();
        }

        //Lấy danh mục sản phẩm
        public List<Category> GetCategories()
        {
            return db.Categories.ToList();
        }

        //TÌm kiếm danh mục sp
        public Category FindCategory(long ID)
        {
            return db.Categories.Find(ID);
        }


        //Lấy sản phẩm gợi ý
        public List<Product> getProductRecommend()
        {

            var lst_pro = new List<Product>();
            var random = new Random();
            int num = random.Next(0, 3);

            if(num == 0)
            {
                lst_pro = db.Products.OrderByDescending(x => x.Product_Name).ToList();
            }else if( num == 1)
            {
                lst_pro = db.Products.OrderBy(x => x.Product_Name).ToList();
            }
            else if (num == 2)
            {
                lst_pro = db.Products.OrderByDescending(x => x.Price).ToList();
            }
            else
            {
                lst_pro = db.Products.OrderBy(x => x.Price).ToList();
            }

            return lst_pro;
        }

        //Chi tiết sản phẩm
        public ProductDTO getProductDetail(long ID)
        {
            var query = from pro in db.Products
                        join cet in db.Categories on pro.Category_ID equals cet.ID
                        join user in db.Users on pro.User_ID equals user.ID
                        select new ProductDTO()
                        {
                            ID = pro.ID,
                            Product_Name = pro.Product_Name,
                            Product_Code = pro.Product_Code,
                            Metatitle = pro.Metatitle.Trim(),
                            Object = pro.Object,
                            Promotion_Price = pro.Promotion_Price,
                            Price = pro.Price,
                            Image = pro.Image,
                            Desription = pro.Desription,
                            Configuration = pro.Configuration,
                            Category_Name = cet.Name,
                            Seller_Name = user.Fullname,
                            Category_ID = pro.Category_ID
                        };
            return query.Single(x => x.ID == ID);
        }


        //Lấy sản phẩm cùng loại
        public IEnumerable<Product> getProductByCategoryID(long? category_id)
        {
            
                return db.Products.Where(x => x.Category_ID == category_id).OrderByDescending(x => x.Product_Name).ToList();
            
        }

        public IEnumerable<Product> getProductByCategory(long? category_id, int page, int pagesize, string order = null)
        {
            if (order != null)
            {
                if (order == "asc")
                    return db.Products.Where(x => x.Category_ID == category_id && x.Status == true).OrderBy(x => x.Promotion_Price).ToPagedList(page, pagesize);
                else
                    return db.Products.Where(x => x.Category_ID == category_id && x.Status == true).OrderByDescending(x => x.Promotion_Price).ToPagedList(page, pagesize);
            }
            else
            {
                return db.Products.Where(x => x.Category_ID == category_id && x.Status == true).OrderByDescending(x => x.Product_Name).ToPagedList(page, pagesize);
            }

        }

        //thêm review sản phẩm
        public bool addReview(Review entity)
        {
            try
            {
                db.Reviews.Add(entity);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        //lấy review sản phẩm
        public List<ReviewDTO> getReview(long product_id)
        {
            var query = from rev in db.Reviews
                        join pro in db.Products on rev.Product_ID equals pro.ID
                        join user in db.Users on rev.User_ID equals user.ID
                        where rev.Product_ID == product_id
                        select new ReviewDTO()
                        {
                            ID = rev.ID,
                            Content = rev.Content,
                            Rating = rev.Rating,
                            Fullname = user.Fullname,
                            CreatedDate = rev.CreatedDate
                        };
            return query.OrderByDescending(x => x.CreatedDate).ToList();
        }


        public List<Product> GetProducts_Recomment(long ID)
        {
            int Support = 2;
            //Khởi tạo luật kết hợp Rule: VD: A => C,B
            List<AssociationRule> rules = new List<AssociationRule>();
            List<AssociationRule> lstLabel = new List<AssociationRule>();
            Apriori apriori = new Apriori(ID); //Load file và Thu gọn các item
            int k = 1; //Tập phổ biến K-Item
            List<ItemSet> ItemSets = new List<ItemSet>(); //Khởi tạo List item 
            bool next;
            do
            {
                next = false;
                //IsFirstItemList: k == 1 : if(k == 1) IsFirstItemList = True
                //                          else IsFirstItemList = False
                var L = apriori.GetItemSet(k, Support, IsFirstItemList: k == 1); //Lấy tập ứng viên sau khi đã kết hợp và so sánh với độ phổ biến support
                if (L.Count > 0)
                {
                    
                    if (k != 1)
                        rules = apriori.GetRules(L); //Các luật kết hợp và tính toán độ tin cậy
                    foreach (var item in rules)
                        lstLabel.Add(item);
                    next = true;
                    k++;
                    ItemSets.Add(L);
                }
            } while (next);

            string label = "";
            foreach (var item in lstLabel)
            {
                if(item.Confidance >= 50)
                {
                   label += item.Label.Replace("=>", " ").Replace(",", " ");
                }

                if (label.Contains(ID.ToString()))
                {
                    label = label.Replace(ID.ToString(), " ");
                }
            }

            List<Product> data = new List<Product>();

            var row = label.Split(' ').ToList();
            foreach (var item in row)
            {
                var product = new Product();
                if(item != ""){
                    product = db.Products.Find(long.Parse(item.Trim()));
                    if (!data.Exists(x => x == product))
                    {
                        data.Add(product);
                    }
                }
                
            }

            return data;
        }
    }
}