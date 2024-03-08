using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Xml.Linq;

namespace FastFoodBranch.App_Start
{
    public class ThrottleAttribute : ActionFilterAttribute
    {
        private readonly int _limit;
        private readonly TimeSpan _period;
        private static readonly MemoryCache _cache = MemoryCache.Default;

        public ThrottleAttribute(int limit, int periodInSeconds)
        {
            _limit = limit;
            _period = TimeSpan.FromSeconds(periodInSeconds);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var requestKey = $"{filterContext.HttpContext.Request.UserHostAddress}-{filterContext.ActionDescriptor.ActionName}";
            //var requestKey = $"{filterContext.HttpContext.Request.UserHostAddress}-{filterContext.ActionDescriptor.ActionName}-{GetClientIp(filterContext.HttpContext.Request)}";
            var cacheItem = _cache.Get(requestKey);

            if (cacheItem == null)
            {
                _cache.Set(requestKey, 1, DateTimeOffset.Now.Add(_period));
                base.OnActionExecuting(filterContext);
            }
            else
            {
                var count = (int)cacheItem;
                if (count >= _limit)
                {
                    filterContext.Result = new HttpStatusCodeResult((int)(HttpStatusCode)429, "Too many requests. Please try again later.");
                    return;
                }

                _cache.Set(requestKey, count + 1, DateTimeOffset.Now.Add(_period));
                base.OnActionExecuting(filterContext);
            }
        }
        private string GetClientIp(HttpRequestBase request)
        {
            string ipAddress = string.Empty;

            if (request != null)
            {
                ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = request.ServerVariables["REMOTE_ADDR"];
                }
                else
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length > 1)
                    {
                        ipAddress = addresses[0];
                    }
                }
            }
            return ipAddress;
        }
    }

}