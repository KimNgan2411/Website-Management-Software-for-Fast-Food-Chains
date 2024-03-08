using FastFoodBranch.App_Start;
using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FastFoodBranch.Areas.Api.Controllers
{
    [Throttle(limit: 5, periodInSeconds: 60)]
    public class TinTucController : Controller
    {
        // GET: Api/TinTuc
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();

        public ActionResult MBDSTinTuc()
        {
            try
            {
                var checkCN = Session["MBlocationCN"];
                if (checkCN != null)
                {
                    var tt = db.BangTins.Where(s => s.LocationID == (string)checkCN && s.TrangThai == 1).ToList();
                    List<DSBangTin> data = tt.Select(ma => new DSBangTin
                    {
                        ID = ma.ID,
                        TieuDe = ma.TieuDe,
                        NoiDung = ma.NoiDung,
                        HinhAnh1 = ma.HinhAnh1,
                        NgayDang = ma.NgayDang
                    }).ToList();
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    var tt = db.BangTins.Where(s => s.TrangThai == 1).ToList();
                    List<DSBangTin> data = tt.Select(ma => new DSBangTin
                    {
                        ID = ma.ID,
                        TieuDe = ma.TieuDe,
                        NoiDung = ma.NoiDung,
                        HinhAnh1 = ma.HinhAnh1,
                        NgayDang = ma.NgayDang
                    }).ToList();
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult MBDetailTinTuc(string idtt, string locationid)
        {
            try
            {
                var tt = db.BangTins.Where(s => s.LocationID == locationid && s.TrangThai == 1 && s.ID == idtt).ToList();
                if (tt.Count != 0)
                {
                   
                    List<DSBangTin> data = tt.Select(ma => new DSBangTin
                    {
                        ID = ma.ID,
                        TieuDe = ma.TieuDe,
                        NoiDung = ma.NoiDung,
                        HinhAnh1 = ma.HinhAnh1,
                        HinhAnh2 = ma.HinhAnh2,
                        HinhAnh3 = ma.HinhAnh3,
                        NgayDang = ma.NgayDang
                    }).ToList();
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, error = "Bảng tin này không tồn tài hoặc đã bị gỡ bỏ!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //Model bảng tin
        private class DSBangTin
        {
            public string ID { get; set; }
            public string TieuDe { get; set; }
            public string NoiDung { get; set; }
            public string HinhAnh1 { get; set; }
            public string HinhAnh2 { get; set; }
            public string HinhAnh3 { get; set; }
            public Nullable<System.DateTime> NgayDang { get; set; }
            public Nullable<bool> Lock { get; set; }
            public Nullable<int> TrangThai { get; set; }
        }
    }
}