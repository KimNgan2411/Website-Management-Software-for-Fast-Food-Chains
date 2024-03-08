using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FastFoodBranch.Models;

namespace FastFoodBranch.Controllers
{
    public class LoginUserController : Controller
    {
         QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        // GET: LoginUser
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string User, string Pass)
        {
            var data = db.NhanViens.Where(s => s.UserName == User && s.Password == Pass).FirstOrDefault() ;
            if (data == null)
            {
                TempData["error"] = "Tài khoản đăng nhập không đúng";
                return View("Login");
            }
            else
            {
                if(data.MaCV == "NV")
                {
                    var check = db.ChiNhanhs.Where(s=>s.NVQL == data.MaNV).FirstOrDefault() ;
                    if(check == null)
                    {
                        TempData["error"] = "Nhân viên này chưa được chỉ định!";
                        return View("Login");
                    }
                }
                //add session
                db.Configuration.ValidateOnSaveEnabled = false;
                Session["userNV"] = data;
                if (data.MaCV.ToString() == "Admin" || data.MaCV.ToString()=="NV")
                {
                    TempData["error"] = "Đăng nhập thành công!";
                    //return View("Login");
                    return RedirectToAction("TrangChu","Admin");
                }
                else
                {
                    TempData["error"] = "Chưa có chức vụ này trong hệ thông";
                    return View("Login");
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LoginKH(string User, string Pass)
        {
            var data = db.KhachHangs.SingleOrDefault(s => (s.UserName == User || s.Email == User) && s.Password == Pass);
            if (data == null)
            {
                TempData["messLG"] = "Tài khoản đăng nhập không đúng";
                return RedirectToAction("LRAccount", "KhachHang");
            }
            else
            {
                //add session
                db.Configuration.ValidateOnSaveEnabled = false;
                Session["userKH"] = data;
                TempData["messLG"] = "Đăng nhập thành công!";
                //return View("Login");
                return RedirectToAction("TrangChu", "KhachHang");
            }
        }

        public ActionResult LogoutKH()
        {
            Session.Clear();//remove session
            Session.Abandon();
            Session.RemoveAll();
            //Thực hiện clear sessionID cũ
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", string.Empty));
            }
            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
                Response.Cookies.Add(new HttpCookie("AuthToken", ""));
            }

            FormsAuthentication.SignOut();
            return RedirectToAction("TrangChu","KhachHang");
        }

        public ActionResult Logout()
        {
            // Xóa session trong ứng dụng
            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();

            //Thực hiện clear sessionID cũ
            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);
                Response.Cookies["ASP.NET_SessionId"].Value = string.Empty;
                Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", string.Empty));
            }
            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies["AuthToken"].Value = string.Empty;
                Response.Cookies["AuthToken"].Expires = DateTime.Now.AddMonths(-20);
                Response.Cookies.Add(new HttpCookie("AuthToken", ""));
            }
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        public ActionResult QuenMK()
        {
            return View();
        }
    }
}