using ShoppingOnline.Models.DTO;
using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.Business
{
    public class UserBusiness
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();

        public User findUser(User user)
        {
            return db.Users.Single(x => x.Account == user.Account && x.Password == user.Password);
        }

        public User findID(long ID)
        {
            return db.Users.Find(ID);
        }

        public User checkLogin(User user)
        {
            var res = db.Users.FirstOrDefault(x => x.Account == user.Account && x.Password == user.Password);
            if(res != null)
            {
                return res;
            }
            else
            {
                return null;
            }
        }

        //Kiểm tra xem đã tồn tại account chưa
        public bool CheckAccount(string account)
        {
            var res = db.Users.Count(x => x.Account == account);
            if (res != 0)
                return true;
            else
                return false;
        }

        public User EmailUser(string email)
        {
            return db.Users.SingleOrDefault(x => x.Account == email);
        }

        public bool AddUserEmail(string email, string fullname)
        {
            try
            {
                var user = new User();
                user.Account = email;
                user.Fullname = fullname;
                user.RoleID = 3;
                user.Status = true;

                db.Users.Add(user);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool register(User entity)
        {
            try
            {
                var user = new User();
                user.Account = entity.Account.Trim();
                user.Password = entity.Password.Trim();
                user.Fullname = entity.Fullname.Trim();
                user.Address = entity.Address;
                user.Phone = entity.Phone;
                user.Email = entity.Email;
                if(entity.RoleID == null)
                {
                    user.RoleID = 3;
                    user.Status = true;
                }
                else
                {
                    user.RoleID = 2;
                    user.Status = false;
                }

                db.Users.Add(user);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void changePass(long ID, string Password)
        {
            var user = db.Users.Find(ID);
            user.Password = Password.Trim();
            db.SaveChanges();
        }

        //check mk trùng hay k
        public bool checkPass(string ex_Password, long ID)
        {
            var user = db.Users.Find(ID);
            if (user.Password.Trim() == ex_Password)
                return true;
            else
                return false;
        }

        //Cập nhật thông tin user
        public bool UpdateUser(User entity)
        {
            try
            {
                var user = db.Users.Find(entity.ID);
                user.Fullname = entity.Fullname;
                user.Phone = entity.Phone;
                user.Address = entity.Address;
                user.Email = entity.Email;
                db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        //lịch sử đơn hàng
        public List<OrderDTO> getOrderHistory(long user_id)
        {
            var query = from order in db.Orders
                        join detail in db.Order_Detail on order.ID equals detail.Order_ID
                        join pro in db.Products on detail.Product_ID equals pro.ID
                        select new OrderDTO()
                        {
                            Order_ID = order.ID,
                            Order_Code = order.Order_Code,
                            CreatedDate = order.CreatedDate,
                            Product_Name = pro.Product_Name,
                            TotalMoney = detail.Price * detail.Quantity,
                            Quantity = order.TotalQuantity,
                            User_ID = order.User_ID,
                            ShipDate = order.ShipDate,
                            Status = order.Status
                        };
            return query.Where(x => x.User_ID == user_id && x.Status != 1 && x.Status != 2).OrderByDescending(x => x.CreatedDate).ToList();
        }

        //Chi tiết đơn hàng
        public List<Order_DetailDTO> order_detail(long ID)
        {
            var query = from order_detail in db.Order_Detail
                        join pro in db.Products on order_detail.Product_ID equals pro.ID
                        where order_detail.Order_ID == ID
                        select new Order_DetailDTO()
                        {
                            ID = order_detail.ID,
                            Product_Name = pro.Product_Name,
                            Quantity = order_detail.Quantity,
                            Price = order_detail.Price,
                            Amount = order_detail.Amount
                        };
            return query.OrderByDescending(x => x.ID).ToList();
        }

        //TÌm đơn hàng
        public Order findOrder(long ID)
        {
            return db.Orders.Find(ID);
        }

        //Đơn hàng chưa thanh toán/chưa giao
        public List<OrderDTO> MyOrder(long user_id)
        {
            var query = from order in db.Orders
                        join detail in db.Order_Detail on order.ID equals detail.Order_ID
                        join pro in db.Products on detail.Product_ID equals pro.ID
                        select new OrderDTO()
                        {
                            Order_ID = order.ID,
                            Order_Code = order.Order_Code,
                            CreatedDate = order.CreatedDate,
                            Product_Name = pro.Product_Name,
                            TotalMoney = detail.Price * detail.Quantity,
                            User_ID = order.User_ID,
                            Quantity = order.TotalQuantity,
                            ShipDate = order.ShipDate,
                            Status = order.Status,
                            Payment = order.Payment
                        };
            return query.Where(x => x.User_ID == user_id && x.Status == 1).OrderByDescending(x => x.CreatedDate).ToList();
        }

        //Hủy đơn hàng
        public void CancerOrder(long ID)
        {
            var order = db.Orders.Find(ID);
            order.Status = 0;
            order.CancerDate = DateTime.Now;
            db.SaveChanges();
        }
    }
}