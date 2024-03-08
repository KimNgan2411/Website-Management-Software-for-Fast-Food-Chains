using FastFoodBranch.App_Start;
using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGrease.Activities;

namespace FastFoodBranch.Areas.Api.Controllers
{
    [Throttle(limit: 5, periodInSeconds: 60)]
    public class MonAnController : Controller
    {
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        // GET: Api/Admin
        public ActionResult Index()
        {
            return Json(new { name = "Danh sách món" }, JsonRequestBehavior.AllowGet);
        }

        //Thông tin chi tiết món ăn
        public ActionResult MBChiTietMA(string id)
        {
         
            try
            {
                if (id == null)
                {
                    return Json(new { success = false, error = "Không thể để id null", result = 0 }, JsonRequestBehavior.AllowGet);
                }
                var location = (string)Session["MBlocationCN"];
                if (location == null)
                {
                    return Json(new { success = false, error = "Người dùng chưa chọn chi nhánh", result = 3 }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var ma = db.MonAns.Find(id);
                    if (ma == null)
                    {
                        return Json(new { success = false, error = "Không tìm thấy món ăn ở chi nhánh này", result = 2 }, JsonRequestBehavior.AllowGet);
                    }

                    db.sp_TangViewMA(id, location);
                    var macn = db.MonAnChiNhanhs.Where(s => s.IDMonAn == id && s.LocationID == location).FirstOrDefault();
                    TempData["slxem"] = macn.SLView;
                    var data = new chitietMonAnCN
                    {
                        ID = macn.ID,
                        TenMonAn = macn.MonAn.TenMA,
                        trangThai = (int)macn.TrangThai,
                        slView = (int)macn.SLView,
                        image = macn.MonAn.HinhAnh
                    };
                    return Json(new { success = true, data = data, result = 1 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message, result = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult MBDSYeuThich(string idkh)
        {
            try
            {
                var dsYeuThich = db.DSYTs.Where(yt => yt.MaKH == idkh).ToList();
                if(dsYeuThich.Count !=0)
                {
                    List<YeuThichViewModel> result = new List<YeuThichViewModel>();
                    foreach (var yeuThich in dsYeuThich)
                    {
                        // Lấy thông tin từ bảng ChiNhanh và MonAn
                        var chiNhanh = db.ChiNhanhs.FirstOrDefault(cn => cn.LocationID == yeuThich.LocationID);
                        var monAn = db.MonAns.FirstOrDefault(ma => ma.ID == yeuThich.IDMonAn);

                        // Kiểm tra xem có thông tin từ cả hai bảng hay không
                        if (chiNhanh != null && monAn != null)
                        {
                            result.Add(new YeuThichViewModel
                            {
                                IDMonAn = yeuThich.IDMonAn,
                                TenCN = chiNhanh.TenCN,
                                TenMA = monAn.TenMA,
                                IDCN = yeuThich.LocationID,
                                HinhAnh = monAn.HinhAnh
                            });
                        }
                    }
                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = true, data =(string)null, }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ThemYeuThich(string id, string user)
        {
            var checkCN = (string)Session["MBlocationCN"];
            try
            {
                if(checkCN != null)
                {
                    int result = db.sp_ThemDSYT(user, id, checkCN);
                    db.SaveChanges();
                    return Json(new { success = true, data = "Thêm vào mục yêu thích thành công!"}, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    return Json(new { success = false, error = "Món ăn này chưa được chọn chi nhánh!" }, JsonRequestBehavior.AllowGet);
                }
                
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.InnerException.Message },JsonRequestBehavior.AllowGet);

            }
        }
        [HttpDelete]
        public JsonResult MBXoaYeuThich(string id, string user, string idCN)
        {
            try
            {
                int result = db.sp_XoaDSYT(user, id, idCN);
                db.SaveChanges();
                return Json(new { success = true, data = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message },JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet]
        public JsonResult MBDSMonAn()
        {
            try
            {
                var checkCN = Session["MBlocationCN"];
                var monanCN = db.MonAnChiNhanhs.Where(s => s.TrangThai == 1 && s.Lock == true).ToList();
                if (checkCN != null)
                {
                    monanCN = db.MonAnChiNhanhs.Where(s => s.LocationID == (string)checkCN && s.TrangThai == 1 && s.Lock == true).ToList();
                }
                List<DanhSachMonAn> data = monanCN.Select(ma => new DanhSachMonAn
                {
                    IDMonAn = ma.ID,
                    TenMA = ma.MonAn.TenMA,
                    MoTa = ma.MonAn.MoTa,
                    GiaGoc = (double)ma.MonAn.GiaGoc,
                    IDDanhMuc = ma.MonAn.IDDanhMuc,
                    IDCongThuc = ma.MonAn.IDCongThuc,
                    HinhAnh = ma.MonAn.HinhAnh
                }).ToList();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult MBDMMonAn(string idDanhMuc)
        {
            try
            {
                var checkCN = Session["MBlocationCN"];
                var monanCN = db.MonAnChiNhanhs.Where(s => s.TrangThai == 1 && s.Lock == true).ToList();
                if (checkCN != null)
                {
                    monanCN = db.MonAnChiNhanhs.Where(s => s.LocationID == (string)checkCN && s.TrangThai == 1 && s.Lock == true).ToList();
                }
                List<DanhSachMonAn> data = monanCN
                    .Where(s=>s.MonAn.IDDanhMuc == idDanhMuc)
                    .Select(ma => new DanhSachMonAn
                    {
                    IDMonAn = ma.MonAn.ID,
                    TenMA = ma.MonAn.TenMA,
                    MoTa = ma.MonAn.MoTa,
                    GiaGoc = (double)ma.MonAn.GiaGoc,
                    IDDanhMuc = ma.MonAn.IDDanhMuc,
                    IDCongThuc = ma.MonAn.IDCongThuc,
                    HinhAnh = ma.MonAn.HinhAnh
                }).ToList();
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.InnerException.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPut]
        public ActionResult Update(int id, MonAn monAn)
        {
            return Json(new { name = "Sửa món " + id, monAn });
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            return Json(new { name = "delete #" + id });
        }
        //Lớp model cho các api trên
        private class chitietMonAnCN
        {
            public string ID { get; set; }
            public string TenMonAn { get; set; }
            public int trangThai { get; set; }
            public double slView { get; set; }
            public string image { get; set; }
        }
        public class YeuThichViewModel
        {
            public string IDMonAn { get; set; }
            public string TenCN { get; set; }
            public string TenMA { get; set; }
            public string IDCN { get; set; }
            public string HinhAnh { get; set; }
        }
        private class DanhSachMonAn
        {
            public string IDMonAn { get; set; }
            public string TenMA { get; set; }
            public string MoTa { get; set; }
            public double GiaGoc { get; set; }
            public string IDDanhMuc { get; set; }
            public string IDCongThuc { get; set; }
            public string HinhAnh { get; set; }
        }
    }
}