using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FastFoodBranch.App_Start
{
    public class AuthenticationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra xem ASP.NET_SessionId có tồn tại trong header không
            string sessionId = filterContext.HttpContext.Request.Headers["ASP.NET_SessionId"];
            var currentSessionId = HttpContext.Current.Session.SessionID;
            if (sessionId == null || currentSessionId != sessionId)
            {
                // Xác thực không thành công, có thể quyết định trả về lỗi hoặc chuyển hướng đến trang đăng nhập
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "Xác thực không hợp lệ.");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}