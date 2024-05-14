using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingOnline.Models.Business
{
    public class InwardBusiness
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();
        public bool addInward(Inward entity)
        {
            try
            {
                entity.Createdate = DateTime.Now;
                db.Inwards.Add(entity);
                db.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void addInward_Detail(Inward_Detail entity)
        {
            db.Inward_Detail.Add(entity);
            db.SaveChanges();
        }
    }
}