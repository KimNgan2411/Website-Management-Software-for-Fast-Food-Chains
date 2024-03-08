using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FastFoodBranch.App_Start
{
    public class Authentication: AuthorizeAttribute
    {
        public string MaChucVu { get; set; }
        public string MaChucVu2 { get; set; }
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            //1.Check session  : Đã đăng nhập vào hệ thống = > cho thực hiện filterContext
            //Ngược lại thì cho trở lại trang đăng nhập 
            NhanVien nvSession = (NhanVien)HttpContext.Current.Session["userNV"];
            if (nvSession == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "LoginUser", action = "Login" }));
            }
            else
            {
                #region 2. Check quyền : Có quyền thì cho thực hiện filterContext
                //Ngược lại thì cho trở lại trang đăng nhập  = > Trang từ chối truy cập
                QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
                var count = db.NhanViens.Count(m => m.MaCV == nvSession.MaCV && (m.MaCV == MaChucVu || m.MaCV == MaChucVu2));
                if (count != 0)
                {
                    return;
                }
                else
                {
                    var returnUrl = filterContext.RequestContext.HttpContext.Request.RawUrl;
                    filterContext.Result = new RedirectToRouteResult(new
                        RouteValueDictionary(
                        new
                        {
                            Controller = "TuChoiTruyCap",
                            action = "TuChoiTruyCap",
                            returnUrl = returnUrl.ToString()
                        }));
                }
                #endregion
            }
            return;

        }
    }
}