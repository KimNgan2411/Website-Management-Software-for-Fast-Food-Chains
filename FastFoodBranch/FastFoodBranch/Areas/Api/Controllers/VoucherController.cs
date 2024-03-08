using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FastFoodBranch.Areas.Api.Controllers
{
    public class VoucherController : Controller
    {
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        // GET: Api/Voucher
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult MBDSVoucher()
        {
            try
            {
                var voucher = db.Vouchers.Where(s=>s.LocationID == null).ToList();
                List<Voucher> data = voucher.Select(ma => new Voucher
                {
                    ID = ma.ID,
                    TenVoucher = ma.TenVoucher,
                    NgayApDung = ma.NgayApDung,
                    NgayHetHan = ma.NgayHetHan,
                    MoTa = ma.MoTa,
                    Lock = ma.Lock,
                    TrangThai = ma.TrangThai,
                    Discount = ma.Discount,
                    ConLai = ma.ConLai
                }).ToList();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}