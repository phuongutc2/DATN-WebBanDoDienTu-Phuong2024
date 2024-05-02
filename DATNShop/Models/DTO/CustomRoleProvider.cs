using ShoppingOnline.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ShoppingOnline.Models.DTO
{
    public class CustomRoleProvider : AuthorizeAttribute
    {
        private Shopping_OnlineEntities db = new Shopping_OnlineEntities();

        public int Type { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var user = HttpContext.Current.Session["admin_login"] as User;
            try
            {
                if (user.Type == Type)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new ViewResult()
            {
                ViewName = "/Areas/Admin/Views/Shared/Error.cshtml"
            };
        }
    }
}