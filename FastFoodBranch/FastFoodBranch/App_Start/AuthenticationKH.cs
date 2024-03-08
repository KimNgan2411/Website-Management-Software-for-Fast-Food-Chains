using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FastFoodBranch.App_Start
{
    public class AuthenticationKH: AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //1.Check session  : Đã đăng nhập vào hệ thống = > cho thực hiện filterContext
            //Ngược lại thì cho trở lại trang đăng nhập 
            KhachHang khSession = (KhachHang)HttpContext.Current.Session["userKH"];
            if (khSession == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "KhachHang", action = "LRAccount" }));
            }
            else
            {
                return;
            }
            return;

        }
    }
}