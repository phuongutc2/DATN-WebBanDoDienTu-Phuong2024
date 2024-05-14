using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.Business
{
    public class OrderBusiness
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();

        //thêm hóa đơn
        public bool addOrder(Order entity, long seller_id)
        {
            try
            {
                var order = new Order();

                var maxid = db.Orders.Max(x => x.ID);
                order.Order_Code = "ORDER0" + (maxid + 1).ToString() + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year;
                order.CustomerName = entity.CustomerName;
                order.CustomerAddress = entity.CustomerAddress;
                order.CustomerPhone = entity.CustomerPhone;
                order.Email = entity.Email;
                //order.TotalMoney = entity.TotalMoney;
                //order.TotalQuantity = entity.TotalQuantity;
                order.CreatedDate = DateTime.Now;
                order.Seller_ID = seller_id;
                order.User_ID = entity.User_ID;
                order.Status = 1;//Chưa xác nhận đơn hàng
                order.Payment = 0;
                db.Orders.Add(order);
                db.SaveChanges();

                return true;
            }
            catch
            {
                return false;
            }
        }

        //update tổng số lượng và tổng tiền đơn hàng
        public void Update_Order(long Order_ID, decimal ? TotalMoney, int Quantity)
        {
            var order = db.Orders.Find(Order_ID);
            order.TotalMoney = TotalMoney;
            order.TotalQuantity = Quantity;
            db.SaveChanges();
        }


        //thêm chi tiết hóa đơn
        public void addOrder_Detail(Order_Detail entity)
        {
                db.Order_Detail.Add(entity);
                db.SaveChanges();
        }

        //lấy Order ID lớn nhất
        public long findMaxID()
        {
            return db.Orders.Max(x => x.ID);
        }
    }
}