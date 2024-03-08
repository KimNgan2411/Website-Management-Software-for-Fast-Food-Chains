using FastFoodBranch.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FastFoodBranch.Models;
using System.IO;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Data.Entity.Core.Objects;
using static System.Net.Mime.MediaTypeNames;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace FastFoodBranch.Controllers
{
    [Authentication(MaChucVu = "Admin", MaChucVu2 = "NV")]
    public class AdminController : Controller
    {
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        // GET: Admin
        public ActionResult TrangChu()
        {
            return View();
        }

        public ActionResult MyInfo()
        {
            var user = (NhanVien)Session["userNV"];
            return View(user);
        }
        //Cập nhật thông tin cá nhân
        [HttpPost]
        public ActionResult UpdateInfo([Bind(Include = "TenNV,SDT,DiaChi,Email,NgaySinh")] NhanVien nv)
        {
            if (ModelState.IsValid)
            {
                var ttnv = (NhanVien)Session["userNV"];
                try
                {
                    db.sp_SuaThongTinNV(ttnv.MaNV,ttnv.Password,nv.TenNV,nv.SDT,nv.DiaChi,nv.Email,ttnv.HinhAnh,nv.NgaySinh,ttnv.GioiTinh,ttnv.MaCV);
                    db.SaveChanges();
                    ttnv = db.NhanViens.Where(s => s.MaNV == ttnv.MaNV).SingleOrDefault();
                    Session["userNV"] = ttnv;
                    return Json(new { success = true, manv = ttnv.MaNV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, manv = ttnv.MaNV, nd = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }

            }
            return HttpNotFound();
        }

        //Phần xử lý khi hình ảnh được gửi lên
        public void LuuAnh(NhanVien sp, HttpPostedFileBase HinhAnh)
        {
            #region Hình ảnh
            //Xác định đường dẫn lưu file : Url tương đói => tuyệt đói
            var urlTuongdoi = "/Content/Avartar/";
            var urlTuyetDoi = Server.MapPath(urlTuongdoi);// Lấy đường dẫn lưu file trên server

            //Check trùng tên file => Đổi tên file  = tên file cũ (ko kèm đuôi)
            //Ảnh.jpg = > ảnh + "-" + 1 + ".jpg" => ảnh-1.jpg

            string fullDuongDan = urlTuyetDoi + HinhAnh.FileName;
            int i = 1;
            while (System.IO.File.Exists(fullDuongDan) == true)
            {
                // 1. Tách tên và đuôi 
                var ten = Path.GetFileNameWithoutExtension(HinhAnh.FileName);
                var duoi = Path.GetExtension(HinhAnh.FileName);
                // 2. Sử dụng biến i để chạy và cộng vào tên file mới
                fullDuongDan = urlTuyetDoi + ten + "-" + i + duoi;
                i++;
                // 3. Check lại 
            }
            #endregion
            //Lưu file (Kiểm tra trùng file)
            HinhAnh.SaveAs(fullDuongDan);
            sp.HinhAnh = urlTuongdoi + Path.GetFileName(fullDuongDan);
        }
        //Tiến hành cập nhật lại hình
        [HttpPost]
        public ActionResult UpAnh(HttpPostedFileBase avatar)
        {
            if (ModelState.IsValid)
            {
                if (avatar != null && avatar.ContentLength > 0)
                {
                    var ttnv = (NhanVien)Session["userNV"];
                    try
                    {
                        LuuAnh(ttnv, avatar);
                        db.SP_UpdateAvarNV(ttnv.MaNV, ttnv.HinhAnh);
                        db.SaveChanges();
                        ttnv = db.NhanViens.Where(s => s.MaNV == ttnv.MaNV).SingleOrDefault();
                        Session["userNV"] = ttnv;
                        return Json(new { success = true, message = "Hình đại diện đã được lưu thành công!" });
                    }
                    catch (Exception e)
                    {
                        return Json(new { success = false, message = "Đã xảy ra lỗi khi lưu hình đại diện!", nd = e.Message });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Không có hình nào được gửi lên!" });
                }
            }
            return Json(new { success = false, message = "Không xác định lỗi" });
        }




        //Nhã
        //Danh sách nguyên liệu
        public ActionResult DSNL()
        {
            var dsnl = db.NguyenLieux.ToList();
            return View(dsnl);
        }
        //Thêm nguyên liệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemNL([Bind(Include = "TenNL,DonVi,TrongLuong,TenNCC,BaoQuan,NgaySX,HanSD")] NguyenLieu nl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemNL(nl.TenNL, nl.TrongLuong, nl.DonVi, nl.TenNCC, nl.BaoQuan, nl.NgaySX, nl.HanSD);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tennl"] = nl.TenNL;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tennl"] = nl.TenNL;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSNL");
            }
            return HttpNotFound();
        }
        //Kiểm tra tên nguyên liệu
        [HttpPost]
        public ActionResult KTTenNL(string TenNL)
        {
            var check = db.NguyenLieux.Where(s => s.TenNL == TenNL).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Chỉnh sửa nguyên liệu
        [HttpPost]
        public ActionResult ChinhSuaNL([Bind(Include = "ID,TenNL,TrongLuong,DonVi,TenNCC,BaoQuan,NgaySX,HanSD")] NguyenLieu nl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaNL(nl.ID, nl.TenNL, nl.TrongLuong, nl.DonVi, nl.TenNCC, nl.BaoQuan, nl.NgaySX, nl.HanSD); ;
                    db.SaveChanges();
                    return Json(new { success = true, manl = nl.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, manl = nl.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi nguyên liệu
        [HttpPost]
        public ActionResult ThuHoiNL(string MaNL)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockNL(MaNL, false);
                    db.SaveChanges();
                    return Json(new { success = true, manl = MaNL }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, manl = MaNL }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi nguyên liệu
        [HttpPost]
        public ActionResult PhucHoiNL(string MaNL)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockNL(MaNL, true);
                    db.SaveChanges();
                    return Json(new { success = true, manl = MaNL }, JsonRequestBehavior.AllowGet); ;
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, manl = MaNL }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa nguyên liệu
        [HttpPost]
        public ActionResult XoaNL(string MaNL)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaNL(MaNL);
                    db.SaveChanges();
                    return Json(new { success = true, manl = MaNL }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, manl = MaNL }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        //Danh sách công thức
        public ActionResult DSCT()
        {
            var dsct = db.CongThucs.ToList();
            return View(dsct);
        }
        //Kiểm tra tên công thức
        [HttpPost]
        public ActionResult KTTenCT(string TenCT)
        {
            var check = db.CongThucs.Where(s => s.TenCT == TenCT).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Thêm công thức
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemCT([Bind(Include = "TenCT,MoTa")] CongThuc ct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemCT(ct.TenCT, ct.MoTa);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tenct"] = ct.TenCT;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tenct"] = ct.TenCT;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSCT");
            }
            return HttpNotFound();
        }
        //Chỉnh sửa công thức
        [HttpPost]
        public ActionResult ChinhSuaCT([Bind(Include = "ID,TenCT,MoTa")] CongThuc ct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaCT(ct.ID, ct.TenCT, ct.MoTa);
                    db.SaveChanges();
                    return Json(new { success = true, mact = ct.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mact = ct.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi công thức
        [HttpPost]
        public ActionResult ThuHoiCT(string MaCT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockCT(MaCT, false);
                    db.SaveChanges();
                    return Json(new { success = true, mact = MaCT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mact = MaCT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi công thức
        [HttpPost]
        public ActionResult PhucHoiCT(string MaCT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockCT(MaCT, true);
                    db.SaveChanges();
                    return Json(new { success = true, mact = MaCT }, JsonRequestBehavior.AllowGet); ;
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mact = MaCT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa công thức
        [HttpPost]
        public ActionResult XoaCT(string MaCT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaCT(MaCT);
                    db.SaveChanges();
                    return Json(new { success = true, mact = MaCT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mact = MaCT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Thêm chi tiết công thức món ăn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemCTCT([Bind(Include = "ID,IDCongThuc")] ChiTietCT ctct)
        {
            int checkDom = int.Parse(Request["soluongDom"]);
            if (checkDom > 0)
            {
                try
                {
                    for (int i = 1; i <= checkDom; i++)
                    {
                        var manl = Request["MaNL" + i];
                        int sl = int.Parse(Request["SoLuong" + i]);
                        db.sp_ThemCTCT(manl, ctct.IDCongThuc, sl);
                        db.SaveChanges();

                    }
                    TempData["messageAlert"] = "success1";
                    TempData["idCT"] = ctct.IDCongThuc;
                    return RedirectToAction("DSCT");
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error1";
                    TempData["idCT"] = ctct.IDCongThuc;
                    TempData["messER"] = e.InnerException.Message;
                    return RedirectToAction("DSCT");
                }
            }
            return HttpNotFound();
        }
        public class ChiTietCTDTO
        {
            public string ID { get; set; }
            public string IDNguyenLieu { get; set; }
            public int SoLuong { get; set; }
        }
        //Lấy danh sách chi tiết công thức
        [HttpPost]
        public JsonResult GetListCTCT(string MaCT)
        {
            try
            {
                var DSCT = db.ChiTietCTs.Where(s => s.IDCongThuc == MaCT);
                var result = DSCT.Select(ct => new ChiTietCTDTO
                {
                    ID = ct.ID,
                    IDNguyenLieu = ct.IDNguyenLieu,
                    SoLuong = (int)ct.SoLuong
                }).ToList();
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChinhSuaCTCT([Bind(Include = "IDCongThuc")] ChiTietCT ctct)
        {
            int checkDom = int.Parse(Request["soluongDom1"]);
            int slbandau = db.ChiTietCTs.Where(s => s.IDCongThuc == ctct.IDCongThuc).Count();
            if (checkDom > 0)
            {
                try
                {
                    //Nếu slban đầu bằng checkDom thì chỉ tiến hành cập nhật
                    if (slbandau == checkDom)
                    {
                        List<ChiTietCTDTO> list = new List<ChiTietCTDTO>();
                        //Lưu các thông tin vào biến tạm trước
                        var dsctct = db.ChiTietCTs.Where(s => s.IDCongThuc == ctct.IDCongThuc).ToList();
                        foreach (var item in dsctct)
                        {
                            list.Add(new ChiTietCTDTO
                            {
                                ID = item.ID,
                                IDNguyenLieu = item.IDNguyenLieu,
                                SoLuong = (int)item.SoLuong
                            });
                        }
                        //Thực hiện cập nhật sau
                        for (int i = 1; i <= list.Count; i++)
                        {
                            var id = Request["IdCongThuc" + i];
                            if (id == null)
                            {
                                id = list[i - 1].ID;
                            }
                            var manl = Request["EditMaNL" + i];
                            int sl = int.Parse(Request["EditSoLuong" + i]);
                            db.sp_ChinhSuaCTCT(id, manl, ctct.IDCongThuc, sl);
                            db.SaveChanges();
                        }


                    }
                    else if (checkDom < slbandau)
                    {
                        List<ChiTietCTDTO> list = new List<ChiTietCTDTO>();
                        //Tiến hành cập nhật các nội dung trong check dom
                        for (int i = 1; i <= checkDom; i++)
                        {
                            var id = Request["IdCongThuc" + i];
                            var manl = Request["EditMaNL" + i];
                            int sl = int.Parse(Request["EditSoLuong" + i]);
                            db.sp_ChinhSuaCTCT(id, manl, ctct.IDCongThuc, sl);
                            db.SaveChanges();
                            list.Add(new ChiTietCTDTO
                            {
                                ID = id,
                                IDNguyenLieu = manl,
                                SoLuong = sl
                            });
                        }
                        //Tiến hành xóa các dom sau đó
                        for (int i = checkDom + 1; i <= slbandau; i++)
                        {

                            //var check = list.Find(s => s.ID == );
                            //db.sp_XoaCTCT(id,ctct.IDCongThuc);
                            //db.SaveChanges();
                        }
                        var dsct = db.ChiTietCTs.Where(s => s.IDCongThuc == ctct.IDCongThuc).ToList();
                        foreach (var item in dsct)
                        {
                            var check = list.Find(s => s.ID == item.ID);
                            if (check == null)
                            {
                                db.sp_XoaCTCT(item.ID, ctct.IDCongThuc);
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (checkDom > slbandau)
                    {

                        //Tiến hành cập nhật các nội dung trong check dom
                        for (int i = 1; i <= slbandau; i++)
                        {
                            var id = Request["IdCongThuc" + i];
                            var manl = Request["EditMaNL" + i];
                            int sl = int.Parse(Request["EditSoLuong" + i]);
                            db.sp_ChinhSuaCTCT(id, manl, ctct.IDCongThuc, sl);
                            db.SaveChanges();

                        }
                        //Tiến hành thêm cập nhật các nội dung mới
                        for (int i = slbandau + 1; i <= checkDom; i++)
                        {
                            var manl = Request["EditMaNL" + i];
                            int sl = int.Parse(Request["EditSoLuong" + i]);
                            db.sp_ThemCTCT(manl, ctct.IDCongThuc, sl);
                            db.SaveChanges();
                        }

                    }
                    //for (int i = 1; i <= checkDom; i++)
                    //{
                    //    var manl = Request["MaNL" + i];
                    //    int sl = int.Parse(Request["SoLuong" + i]);
                    //    db.sp_ThemCTCT(manl, ctct.IDCongThuc, sl);
                    //    db.SaveChanges();

                    //}


                    TempData["messageAlert"] = "success2";
                    TempData["idCT"] = ctct.IDCongThuc;
                    return RedirectToAction("DSCT");
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error2";
                    TempData["idCT"] = ctct.IDCongThuc;
                    TempData["messER"] = e.InnerException.Message;
                    return RedirectToAction("DSCT");
                }
            }
            return HttpNotFound();
        }



        //Danh sách món ăn
        public ActionResult DSMA()
        {
            var dsma = db.MonAns.ToList();
            return View(dsma);
        }
        //Thêm món ăn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemMA([Bind(Include = "TenMA,MoTa,GiaGoc,IDDanhMuc,IDCongThuc,HinhAnh")] MonAn ma, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnh != null)
                    {
                        LuuAnh(ma, HinhAnh);
                    }
                    ma.HinhAnh = null;
                    db.sp_ThemMA(ma.TenMA, ma.MoTa, ma.GiaGoc, ma.IDDanhMuc, ma.IDCongThuc, ma.HinhAnh);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tenma"] = ma.TenMA;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tenma"] = ma.TenMA;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSMA");
            }
            return HttpNotFound();
        }
        //Kiểm tra tên món ăn
        [HttpPost]
        public ActionResult KTTenMA(string TenMA)
        {
            var check = db.MonAns.Where(s => s.TenMA == TenMA).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Chỉnh sửa món ăn
        [HttpPost]
        public ActionResult ChinhSuaMA([Bind(Include = "ID,TenMA,MoTa,GiaGoc,IDDanhMuc,IDCongThuc,HinhAnh")] MonAn ma, HttpPostedFileBase HinhAnh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (HinhAnh != null)
                    {
                        LuuAnh(ma, HinhAnh);
                    }
                    else
                    {
                        var ha = db.MonAns.Where(s => s.ID == ma.ID).FirstOrDefault();
                        ma.HinhAnh = ha.HinhAnh;
                    }

                    db.sp_ChinhSuaMA(ma.ID, ma.TenMA, ma.MoTa, ma.GiaGoc, ma.IDDanhMuc, ma.IDCongThuc, ma.HinhAnh);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success1";
                    TempData["tenma"] = ma.TenMA;
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error1";
                    TempData["tenma"] = ma.TenMA;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSMA");
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi món ăn
        [HttpPost]
        public ActionResult ThuHoiMA(string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockMA(MaMA, false);
                    db.SaveChanges();
                    return Json(new { success = true, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi món ăn
        [HttpPost]
        public ActionResult PhucHoiMA(string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockMA(MaMA, true);
                    db.SaveChanges();
                    return Json(new { success = true, mama = MaMA }, JsonRequestBehavior.AllowGet); ;
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa món ăn
        [HttpPost]
        public ActionResult XoaMA(string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaMA(MaMA);
                    db.SaveChanges();
                    return Json(new { success = true, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Danh sách danh mục
        public ActionResult DSDM()
        {
            var dsdm = db.DanhMucs.ToList();
            return View(dsdm);
        }

        //Thêm danh mục
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDM([Bind(Include = "TenDanhMuc")] DanhMuc dm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemDanhMuc(dm.TenDanhMuc);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tendm"] = dm.TenDanhMuc;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error01";
                    TempData["tendm"] = dm.TenDanhMuc;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSDM");
            }
            return HttpNotFound();
        }
        //Chỉnh sửa danh mục
        [HttpPost]
        public ActionResult ChinhSuaDM([Bind(Include = "ID,TenDanhMuc")] DanhMuc dm)
        {
            var user = (NhanVien)Session["user"];
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaDanhMuc(dm.ID, dm.TenDanhMuc);
                    db.SaveChanges();
                    return Json(new { success = true, madm = dm.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, madm = dm.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi danh mục
        [HttpPost]
        public ActionResult ThuHoiDM(string MaDM)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockDM(MaDM, false);
                    db.SaveChanges();
                    return Json(new { success = true, madm = MaDM }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, madm = MaDM }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi danh mục
        [HttpPost]
        public ActionResult PhucHoiDM(string MaDM)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockDM(MaDM, true);
                    db.SaveChanges();
                    return Json(new { success = true, madm = MaDM }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, madm = MaDM }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Hàm xóa danh mục
        [HttpPost]
        public ActionResult XoaDM(string MaDM)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaDanhMuc(MaDM);
                    db.SaveChanges();
                    return Json(new { success = true, madm = MaDM }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, madm = MaDM }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Hàm kiểm tra tên danh mục
        [HttpPost]
        public ActionResult KTTenDM(string TenDanhMuc)
        {
            var check = db.DanhMucs.Where(s => s.TenDanhMuc == TenDanhMuc).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }


        //Danh sách biến động giá
        public ActionResult QLG()
        {
            var qlg = db.BienDongGias.ToList();
            return View(qlg);
        }

        //Thêm biến động gia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemGia([Bind(Include = "IDmonAn,GiaApDung,NgayApDung,NgayHetHan")] BienDongGia bdg)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemBDG(bdg.IDMonAn, bdg.GiaApDung, bdg.NgayApDung, bdg.NgayHetHan);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tenma"] = bdg.IDMonAn;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tenma"] = bdg.IDMonAn;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("QLG");
            }
            return HttpNotFound();
        }
        //Chỉnh sửa giá của món ăn
        [HttpPost]
        public ActionResult ChinhSuaGiaBDG([Bind(Include = "ID,IDMonAn,GiaApDung")] BienDongGia bdg)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaGiaBDG(bdg.ID, bdg.IDMonAn, bdg.GiaApDung);
                    db.SaveChanges();
                    return Json(new { success = true, mabdg = bdg.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mabdg = bdg.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi giá của món ăn này
        [HttpPost]
        public ActionResult ThuHoiGiaMA(string ID, string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockBDG(ID, MaMA, false);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi giá của món ăn này
        [HttpPost]
        public ActionResult PhucHoiBDG(string ID, string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockBDG(ID, MaMA, true);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa giá của món ăn
        [HttpPost]
        public ActionResult XoaGia(string ID, string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaBDG(ID, MaMA);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }



        //Danh sách tồn kho
        public ActionResult DSHH()
        {
            var qlg = db.ChiTietKhoes.Where(s => s.IDKho == "KH0").ToList();
            return View(qlg);
        }
        //Danh sách phiếu nhập
        public ActionResult DSPN()
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {
                var location = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var kho = db.Khoes.Where(s=>s.LocationID == location.LocationID).FirstOrDefault();
                var dspn = db.PhieuNhapXuats.Where(s => s.IDKho == kho.ID).ToList();
                return View(dspn);
            }


            var qlg = db.PhieuNhapXuats.ToList();
            return View(qlg);
        }

        //Thêm phiếu nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemPN([Bind(Include = "IDKho,IDNV,LoaiPhieu,NgayLap")] PhieuNhapXuat pn)
        {
            var user = (NhanVien)Session["userNV"];
            if (ModelState.IsValid)
            {
                try
                {
                    ObjectParameter maphieuPN = new ObjectParameter("maphieu", typeof(String));
                    ObjectParameter maphieuPX = new ObjectParameter("maphieu", typeof(String));
                    if (pn.IDKho == null)
                    {
                        pn.IDKho = "KH0";
                    }

                    pn.NgayLap = System.DateTime.Now;
                    db.sp_ThemPNX(pn.IDKho, user.MaNV, true, pn.NgayLap, maphieuPN);
                    db.SaveChanges();



                    int checkDom = int.Parse(Request["soluongDom"]);
                    if (checkDom > 0)
                    {
                        for (int i = 1; i <= checkDom; i++)
                        {
                            ChiTietPNX ctpnx = new ChiTietPNX();
                            ctpnx.IDPNX = Convert.ToString(maphieuPN.Value);
                            ctpnx.IDNguyenLieu = Request["MaNL" + i];
                            ctpnx.SoLuong = Convert.ToInt32(Request["SoLuong" + i]);
                            db.sp_ThemCTPNX(ctpnx.IDPNX, ctpnx.IDNguyenLieu, ctpnx.SoLuong);
                            //Kiểm tra số lượng sản phẩm tồn trong kho
                        }
                        if (pn.IDKho != "KH0")
                        {
                            //Tạo thêm phiếu xuất cho kho tổng
                            db.sp_ThemPNX("KH0", user.MaNV, false, pn.NgayLap, maphieuPX);
                            for (int i = 1; i <= checkDom; i++)
                            {
                                ChiTietPNX ctpx = new ChiTietPNX();
                                ctpx.IDPNX = Convert.ToString(maphieuPX.Value);
                                ctpx.IDNguyenLieu = Request["MaNL" + i];
                                ctpx.SoLuong = Convert.ToInt32(Request["SoLuong" + i]);
                                db.sp_ThemCTPNX(ctpx.IDPNX, ctpx.IDNguyenLieu, ctpx.SoLuong);
                                //Kiểm tra số lượng sản phẩm tồn trong kho
                            }
                        }
                    }
                    TempData["messageAlert"] = "success0";
                    TempData["mapn"] = maphieuPN.Value;
                    if (pn.IDKho == "KH0")
                    {
                        //TempData["manl"] = ;
                        return RedirectToAction("DSHH");
                    }
                    else
                    {
                        var location = db.Khoes.Where(s => s.ID == pn.IDKho).FirstOrDefault();
                        //TempData["manl"] = ;
                        TempData["messageAlert"] = "success0";
                        TempData["tencn"] = location.ChiNhanh.TenCN;
                        return RedirectToAction("KhoCN", new { id = location.LocationID });
                    }
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["mapn"] = pn.ID;
                    TempData["mess"] = e.InnerException.Message;
                    if (pn.IDKho == "KH0")
                    {
                        //TempData["manl"] = pn.IDNguyenLieu;
                        return RedirectToAction("DSHH");
                    }
                    else
                    {
                        var location = db.Khoes.Where(s => s.ID == pn.IDKho).FirstOrDefault();
                        TempData["messageAlert"] = "success0";
                        TempData["tencn"] = location.ChiNhanh.TenCN;
                        return RedirectToAction("KhoCN", new { id = location.LocationID });
                    }
                }
            }
            return HttpNotFound();
        }

        //Xét duyệt phiếu nhập xuất 
        [HttpPost]
        public ActionResult DuyetPhieu(string MaPhieu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    
                    //Xét duyệt phiếu nhập
                    db.sp_XetDuyetPNX2(MaPhieu,1);
                    db.SaveChanges();

                    return Json(new { success = true, maphieu = MaPhieu }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maphieu = MaPhieu }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Từ chối món ăn chi nhánh
        [HttpPost]
        public ActionResult TuchoiPhieu(string MaPhieu)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_TuChoiPN(MaPhieu);
                    db.SaveChanges();
                    return Json(new { success = true, maphieu = MaPhieu }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maphieu = MaPhieu }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }




        //Chỉnh sửa phiếu nhập

        //Thu hồi phiếu nhập
        [HttpPost]
        public ActionResult ThuHoiPN(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //db.sp_LockPN(ID, false);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi phiếu nhập
        [HttpPost]
        public ActionResult PhucHoiPN(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //db.sp_LockPN(ID, true);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa phiếu nhập
        [HttpPost]
        public ActionResult XoaPN(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //db.sp_XoaPN(ID);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Hàm lấy danh sách chi tiết phiếu nhập
        [HttpPost]
        public JsonResult GetListCTPN(string MaPN)
        {
            try
            {
                var DSCT = db.ChiTietPNXes.Where(s => s.IDPNX == MaPN);
                var result = DSCT.Select(ct => new ChiTietCTDTO
                {
                    ID = ct.ID,
                    IDNguyenLieu = ct.IDNguyenLieu,
                    SoLuong = (int)ct.SoLuong
                }).ToList();
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //Danh sách phiếu xuất
        public ActionResult DSPX()
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {

                var dspx = db.PhieuNhapXuats.Where(s => s.IDNV == user.MaNV).ToList();
                return View(dspx);
            }

            var qlg = db.PhieuNhapXuats.ToList();
            return View(qlg);
        }
        //Thêm phiếu xuất
        //Chỉnh sửa phiếu xuất

        //Thu hồi phiếu xuất
        [HttpPost]
        public ActionResult ThuHoiPX(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockPX(ID, false);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi phiếu xuất
        [HttpPost]
        public ActionResult PhucHoiPX(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockPX(ID, true);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa phiếu xuất
        [HttpPost]
        public ActionResult XoaPX(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaPX(ID);
                    db.SaveChanges();
                    return Json(new { success = true, id = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, id = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Lấy danh sách chi tiết phiếu xuất 
        [HttpPost]
        public JsonResult GetListCTPX(string MaPX)
        {
            try
            {
                var DSCT = db.ChiTietPNXes.Where(s => s.IDPNX == MaPX);
                var result = DSCT.Select(ct => new ChiTietCTDTO
                {
                    ID = ct.ID,
                    IDNguyenLieu = ct.IDNguyenLieu,
                    SoLuong = (int)ct.SoLuong
                }).ToList();
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //Phần xử lý khi hình ảnh món ăn được gửi lên
        public void LuuAnh(MonAn sp, HttpPostedFileBase HinhAnh)
        {
            #region Hình ảnh
            //Xác định đường dẫn lưu file : Url tương đói => tuyệt đói
            var urlTuongdoi = "/Content/AnhMA/";
            var urlTuyetDoi = Server.MapPath(urlTuongdoi);// Lấy đường dẫn lưu file trên server

            //Check trùng tên file => Đổi tên file  = tên file cũ (ko kèm đuôi)
            //Ảnh.jpg = > ảnh + "-" + 1 + ".jpg" => ảnh-1.jpg

            string fullDuongDan = urlTuyetDoi + HinhAnh.FileName;
            int i = 1;
            while (System.IO.File.Exists(fullDuongDan) == true)
            {
                // 1. Tách tên và đuôi 
                var ten = Path.GetFileNameWithoutExtension(HinhAnh.FileName);
                var duoi = Path.GetExtension(HinhAnh.FileName);
                // 2. Sử dụng biến i để chạy và cộng vào tên file mới
                fullDuongDan = urlTuyetDoi + ten + "-" + i + duoi;
                i++;
                // 3. Check lại 
            }
            #endregion
            //Lưu file (Kiểm tra trùng file)
            HinhAnh.SaveAs(fullDuongDan);
            sp.HinhAnh = urlTuongdoi + Path.GetFileName(fullDuongDan);
        }

        //Danh sách chi nhánh
        public ActionResult DSCN()
        {
            var dscn = db.ChiNhanhs.ToList();
            return View(dscn);
        }
        //Hàm kiểm tra tên chi nhánh
        [HttpPost]
        public ActionResult KTTenCN(string TenCN)
        {
            var check = db.ChiNhanhs.Where(s => s.TenCN == TenCN).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Hàm kiểm tra địa chỉ chi nhánh
        [HttpPost]
        public ActionResult KTDiaChiCN(string DiaChiCN)
        {
            var check = db.ChiNhanhs.Where(s => s.DiaChi == DiaChiCN).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Thêm chi nhánh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemCN([Bind(Include = "TenCN,DiaChi,NVQL")] ChiNhanh cn)
        {
            var user = (NhanVien)Session["userNV"];
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemCN(cn.TenCN, cn.DiaChi, cn.NVQL);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tencn"] = cn.TenCN;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tencn"] = cn.TenCN;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSCN");
            }
            return HttpNotFound();
        }
        //Chỉnh sửa chi nhánh
        [HttpPost]
        public ActionResult ChinhSuaCN([Bind(Include = "LocationID,TenCN,DiaChi,NVQL")] ChiNhanh cn)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaCN(cn.LocationID, cn.TenCN, cn.DiaChi, cn.NVQL);
                    db.SaveChanges();
                    return Json(new { success = true, macn = cn.LocationID, tencn = cn.TenCN }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, macn = cn.LocationID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi chi nhánh
        [HttpPost]
        public ActionResult ThuHoiCN(string MaCN)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockCN(MaCN, false);
                    db.SaveChanges();
                    return Json(new { success = true, macn = MaCN }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, macn = MaCN }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi chi nhánh
        [HttpPost]
        public ActionResult PhucHoiCN(string MaCN)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockCN(MaCN, true);
                    db.SaveChanges();
                    return Json(new { success = true, macn = MaCN }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, macn = MaCN }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        // Xóa chi nhánh
        [HttpPost]
        public ActionResult XoaCN(string MaCN)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaCN(MaCN);
                    db.SaveChanges();
                    return Json(new { success = true, macn = MaCN }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, macn = MaCN }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Kho của chi nhánh
        //Danh sách nguyên liệu của chi nhánh
        public ActionResult KhoCN(string id)
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {
                var cn = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var idKho = db.Khoes.Where(s => s.LocationID == cn.LocationID).FirstOrDefault();
                var dsknl = db.ChiTietKhoes.Where(s => s.IDKho == idKho.ID).ToList();
                return View(dsknl);
            }

            if (id != null)
            {
                var idKho = db.Khoes.Where(s => s.LocationID == id).FirstOrDefault();
                var dsknl = db.ChiTietKhoes.Where(s => s.IDKho == idKho.ID).ToList();
                if (dsknl != null)
                {
                    return View(dsknl);
                }
                else
                {
                    return View();
                }
            }
            return View();
        }
        public class ChiTietKho
        {
            public string ID { get; set; }
            public string IDNguyenLieu { get; set; }
            public int SoLuong { get; set; }
        }
        [HttpPost]
        public JsonResult getDataKhoCN(string idKho)
        {
            var dsnl = db.ChiTietKhoes.Where(s => s.IDKho == idKho).ToList();
            List<ChiTietKho> result = dsnl.Select(ct => new ChiTietKho
            {
                ID = ct.ID,
                IDNguyenLieu = ct.NguyenLieu.TenNL,
                SoLuong = (int)ct.SoLuong
            }).ToList();
            if (dsnl != null)
            {
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
        }

        //Danh sách kho chi nhánh
        public ActionResult MonAnCN(string id)
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {
                var cn = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var dsmacn = db.MonAnChiNhanhs.Where(s => s.LocationID == cn.LocationID).ToList();
                return View(dsmacn);
            }

            if (id != null)
            {
                var dsmacn = db.MonAnChiNhanhs.Where(s => s.LocationID == id).ToList();
                if (dsmacn != null)
                {
                    return View(dsmacn);
                }
                else
                {
                    return View();
                }
            }
            return View();
        }
        //Danh sách mặt hàng thường mua cùng nhau
        [HttpPost]
        public JsonResult DanhSachMuaCung(string MaMaCN, string MaCN)
        {
            try
            {
                var MonAn = db.MonAnChiNhanhs.Where(s => s.ID == MaMaCN).FirstOrDefault();
                string idMonAn = MonAn.IDMonAn; // Giá trị IDMonAn bạn quan tâm


                var query = @"SELECT cthd1.IDMonAn AS MonAn1, 
                            cthd2.IDMonAn AS MonAn2, 
                            COUNT(*) AS SoLanMuaCungNhau,
                            ma1.TenMA AS TenMonAn1,
                            ma1.HinhAnh AS HinhAnh1,
                            ma2.TenMA AS TenMonAn2,
                            ma2.HinhAnh AS HinhAnh2
                        FROM 
                            HoaDon hd
                        INNER JOIN 
                            ChiTietHD cthd1 ON hd.ID = cthd1.IDHoaDon
                        INNER JOIN 
                            ChiTietHD cthd2 ON cthd1.IDHoaDon = cthd2.IDHoaDon AND cthd1.IDMonAn != cthd2.IDMonAn
                        INNER JOIN 
                            MonAn ma1 ON cthd1.IDMonAn = ma1.ID
                        INNER JOIN 
                            MonAn ma2 ON cthd2.IDMonAn = ma2.ID
                        WHERE 
                            hd.LocationID = @locationID
                            AND hd.TrangThai = 1
                            AND (cthd1.IDMonAn = @IdMonAn)
                        GROUP BY 
                            cthd1.IDMonAn, 
                            cthd2.IDMonAn,
                            ma1.TenMA,
                            ma1.HinhAnh,
                            ma2.TenMA,
                            ma2.HinhAnh
                        HAVING 
                            COUNT(*) > 1;";
                var result = db.Database.SqlQuery<YourNonEntityType>(query,
                    new SqlParameter("locationID", MaCN),
                    new SqlParameter("IdMonAn", idMonAn)).ToList();



                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, nd = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }
        public class YourNonEntityType
        {
            public string MonAn1 { get; set; }
            public string MonAn2 { get; set; }
            public int SoLanMuaCungNhau { get; set; }
            public string TenMonAn1 { get; set; }
            public string HinhAnh1 { get; set; }
            public string TenMonAn2 { get; set; }
            public string HinhAnh2 { get; set; }
        }

        //Lấy thông tin hiển thị cho món ăn chi nhánh
        public class chitietMonAnCN
        {
            public string ID { get; set; }
            public string TenMonAn { get; set; }
            public int trangThai { get; set; }
            public double slView { get; set; }
            public double slMua { get; set; }
            public double tlMH { get; set; }
            public string image { get; set; }
        }
        [HttpPost]
        public JsonResult getDataMonAnCN(string MaCN, bool Lock01)
        {
            var dsMonAnCN = db.MonAnChiNhanhs.Where(s => s.LocationID == MaCN && s.Lock == Lock01).ToList();
            List<chitietMonAnCN> result = dsMonAnCN.Select(ma => new chitietMonAnCN
            {
                ID = ma.ID,
                TenMonAn = ma.MonAn.TenMA,
                trangThai = (int)ma.TrangThai,
                slView = (double)ma.SLView,
                slMua = (double)ma.SLMua,
                tlMH = ma.SLView != 0 ? (double)((ma.SLMua / (ma.SLView * 1.0)) * 100) : 0,
                image = ma.MonAn.HinhAnh
            }).ToList();
            if (dsMonAnCN != null)
            {
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
        }
        //Lấy thông tin công thức của món ăn
        [HttpGet]
        public JsonResult getDataCTMA(string MaMACN, string MaCN)
        {
            try
            {
                var chiTietMonAn = db.MonAnChiNhanhs
                .Include(c => c.MonAn.CongThuc.ChiTietCTs)
                .Where(s => s.LocationID == MaCN && s.ID == MaMACN)
                .FirstOrDefault();

                if (chiTietMonAn != null)
                {
                    var result = chiTietMonAn.MonAn.CongThuc.ChiTietCTs.Select(ct => new
                    {
                        TenNguyenLieu = ct.NguyenLieu.TenNL,
                        SoLuong = ct.SoLuong
                    }).ToList();
                    return Json(new { success = true, data = result,TenMA = chiTietMonAn.MonAn.TenMA }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
                }
            }catch(Exception ex) {
                return Json(new { success = false, message = "Khong co du lieu tim thay",nd = ex.Message.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }

        //Thu hồi món ăn của chi nhánh
        [HttpPost]
        public ActionResult ThuHoiMonAnCN(string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockMACN(id, false);
                    db.SaveChanges();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PhucHoiMonAnCN(string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockMACN(id, true);
                    db.SaveChanges();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa món ăn của chi nhánh
        [HttpPost]
        public ActionResult XoaMonAnCN(string id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaMACN(id);
                    db.SaveChanges();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Kiểm tra món ăn này có tồn tại ở chi nhánh này chưa 
        [HttpPost]
        public ActionResult KTMonAnCN(string MaCN, string MaMA)
        {
            var check = db.MonAnChiNhanhs.Where(s => s.LocationID == MaCN && s.IDMonAn == MaMA).FirstOrDefault();
            if (check is null)
            {
                //Kiểm tra món ăn này trong chi nhánh đó có đáp ứng được hay không
                var monan = db.MonAns.Where(s => s.ID == MaMA).FirstOrDefault();
                var ctct = db.ChiTietCTs.Where(s=> s.IDCongThuc == monan.IDCongThuc).ToList();
                var khocn = db.Khoes.Where(s=>s.LocationID == MaCN).FirstOrDefault();
                var ctkho = db.ChiTietKhoes.Where(s=> s.IDKho == khocn.ID).FirstOrDefault();
                foreach(var item in ctct)
                {
                    var ktNL = db.ChiTietKhoes.Where(s => s.IDKho == khocn.ID && s.IDNguyenLieu == item.IDNguyenLieu).FirstOrDefault();
                    if(ktNL is null)
                    {
                        return Json(new { success = false,result = 1 }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        continue;
                    }
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, result = 0 }, JsonRequestBehavior.AllowGet);
            }
        }
        //Thêm món ăn cho cho chi nhánh
        [HttpPost]
        public ActionResult ThemMACN([Bind(Include = "ID,IDMonAn,LocationID")] MonAnChiNhanh macn)
        {
            int checkDom = int.Parse(Request["soluongDom"]);
            var user = (NhanVien)Session["userNV"];
            if (checkDom > 0)
            {
                try
                {
                    for (int i = 1; i <= checkDom; i++)
                    {
                        var mama = Request["MaMA" + i];
                        db.sp_ThemMACN(mama, macn.LocationID, user.MaNV);
                        db.SaveChanges();

                    }
                    TempData["messageAlert"] = "success0";
                    TempData["tencn"] = macn.LocationID;
                    return RedirectToAction("MonAnCN", new { id = macn.LocationID });
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tencn"] = macn.LocationID;
                    TempData["messER"] = e.InnerException.Message;
                    return RedirectToAction("MonAnCN", new { id = macn.LocationID });
                }
            }
            return HttpNotFound();
        }
        //Xét duyệt món ăn chi nhánh
        [HttpPost]
        public ActionResult DuyetMACN(string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetMACN(MaMA, 1);
                    db.SaveChanges();
                    return Json(new { success = true, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Từ chối món ăn chi nhánh
        [HttpPost]
        public ActionResult TuchoiMACN(string MaMA)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetMACN(MaMA, 2);
                    db.SaveChanges();
                    return Json(new { success = true, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mama = MaMA }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        //Danh sách tin tức
        public ActionResult DSTT()
        {
            var user = (NhanVien)Session["userNV"];
            if(user.MaCV == "Admin")
            {
                var dstt = db.BangTins.ToList();
                return View(dstt);
            }
            else
            {
                var location = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var dstt = db.BangTins.Where(s=>s.LocationID == location.LocationID).ToList();
                return View(dstt);
            }
        }
        //Xét duyệt Tin tức chi nhánh
        [HttpPost]
        public ActionResult DuyetTTCN(string MaTT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetTTCN(MaTT, 1);
                    db.SaveChanges();
                    return Json(new { success = true, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Từ chối tin tức chi nhánh
        [HttpPost]
        public ActionResult TuchoiTTCN(string MaTT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetTTCN(MaTT, 2);
                    db.SaveChanges();
                    return Json(new { success = true, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Kiểm tra tên tiêu đề
        [HttpPost]
        public ActionResult KTTieuDe(string TenTD)
        {
            var check = db.BangTins.Where(s => s.TieuDe == TenTD).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Thêm tin tức
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemTT([Bind(Include = "TieuDe,NoiDung,HinhAnh1,HinhAnh2,HinhAnh3,LocationID")] BangTin bt, HttpPostedFileBase HinhAnh1, HttpPostedFileBase HinhAnh2, HttpPostedFileBase HinhAnh3)
        {
            var user = (NhanVien)Session["userNV"];
            if (ModelState.IsValid)
            {
                try
                {
                    DSHinhAnh hinhAnh = new DSHinhAnh();
                    hinhAnh.HinhAnh1 = HinhAnh1;
                    hinhAnh.HinhAnh2 = HinhAnh2;
                    hinhAnh.HinhAnh3 = HinhAnh3;
                    LuuAnhTT(bt, hinhAnh);
                    var today = System.DateTime.Now;
                    if(user.MaCV == "NV")
                    {
                        var location = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                        db.sp_ThemTT(bt.TieuDe, bt.NoiDung, bt.HinhAnh1, bt.HinhAnh2, bt.HinhAnh3, location.LocationID, user.MaNV, today);
                    }
                    else
                    {
                        db.sp_ThemTT(bt.TieuDe, bt.NoiDung, bt.HinhAnh1, bt.HinhAnh2, bt.HinhAnh3, bt.LocationID, user.MaNV, today);
                    }
                    
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tieude"] = bt.TieuDe;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tieude"] = bt.TieuDe;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSTT");
            }
            return HttpNotFound();
        }
        //Lấy thông tin tức từng chi nhánh
        [HttpPost]
        public JsonResult getDataTinTucCN(string MaCN, bool Lock01)
        {
            var dstinTuc = db.BangTins.Where(s => s.LocationID == MaCN && s.Lock == Lock01).ToList();
            List<BangTin> result = dstinTuc.Select(ma => new BangTin
            {
                ID = ma.ID,
                TieuDe = ma.TieuDe,
                NoiDung = ma.NoiDung,
                ChiNhanh = ma.ChiNhanh,
                MaNV = ma.MaNV,
                Lock = ma.Lock,
                TrangThai = ma.TrangThai,
                NgayDang = ma.NgayDang,
            }).ToList();
            if (dstinTuc != null)
            {
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
        }

        public class DSHinhAnh
        {
            public HttpPostedFileBase HinhAnh1 { get; set; }
            public HttpPostedFileBase HinhAnh2 { get; set; }
            public HttpPostedFileBase HinhAnh3 { get; set; }
        }
        //Phần xử lý khi hình ảnh món ăn được gửi lên
        public void LuuAnhTT(BangTin tt, DSHinhAnh hinhAnh)
        {
            #region Hình ảnh
            //Xác định đường dẫn lưu file : Url tương đói => tuyệt đói
            var urlTuongdoi = "/Content/AnhMA/";
            var urlTuyetDoi = Server.MapPath(urlTuongdoi);// Lấy đường dẫn lưu file trên server

            //Check trùng tên file => Đổi tên file  = tên file cũ (ko kèm đuôi)
            foreach (var prop in hinhAnh.GetType().GetProperties())
            {
                var item = (HttpPostedFileBase)prop.GetValue(hinhAnh);
                if (item != null)
                {
                    //Ảnh.jpg = > ảnh + "-" + 1 + ".jpg" => ảnh-1.jpg
                    string fullDuongDan = urlTuyetDoi + item.FileName;
                    int i = 1;

                    while (System.IO.File.Exists(fullDuongDan) == true)
                    {
                        // 1. Tách tên và đuôi 
                        var ten = Path.GetFileNameWithoutExtension(item.FileName);
                        var duoi = Path.GetExtension(item.FileName);
                        // 2. Sử dụng biến i để chạy và cộng vào tên file mới
                        fullDuongDan = urlTuyetDoi + ten + "-" + i + duoi;
                        i++;
                        // 3. Check lại 
                    }
                    // Lưu file(Kiểm tra trùng file)
                    item.SaveAs(fullDuongDan);
                    // Xác định tên thuộc tính tương ứng trong tt(Ví dụ: HinhAnh1, HinhAnh2, HinhAnh3)
                    string propertyName = prop.Name;
                    var ttProperty = tt.GetType().GetProperty(propertyName);
                    string valHA = urlTuongdoi + Path.GetFileName(fullDuongDan);
                    // Gán giá trị từ DSHinhAnh sang tt
                    if (ttProperty != null)
                    {
                        ttProperty.SetValue(tt, valHA);
                    }
                }
            }
            #endregion
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChinhSuaTT([Bind(Include = "ID,TieuDe,NoiDung,HinhAnh1,HinhAnh2,HinhAnh3,LocationID")] BangTin bt, HttpPostedFileBase HinhAnh1, HttpPostedFileBase HinhAnh2, HttpPostedFileBase HinhAnh3)
        {
            var user = (NhanVien)Session["userNV"];
            if (ModelState.IsValid)
            {
                try
                {
                    DSHinhAnh hinhAnh = new DSHinhAnh();
                    if (HinhAnh1 != null && HinhAnh2 != null && HinhAnh3 != null)
                    {
                        hinhAnh.HinhAnh1 = HinhAnh1;
                        hinhAnh.HinhAnh2 = HinhAnh2;
                        hinhAnh.HinhAnh3 = HinhAnh3;
                        LuuAnhTT(bt, hinhAnh);
                    }
                    else if (HinhAnh1 == null && HinhAnh2 == null && HinhAnh3 == null)
                    {
                        var ha = db.BangTins.Where(s => s.ID == bt.ID).FirstOrDefault();
                        bt.HinhAnh1 = ha.HinhAnh1;
                        bt.HinhAnh2 = ha.HinhAnh2;
                        bt.HinhAnh3 = ha.HinhAnh3;
                    }
                    else if (HinhAnh1 == null && HinhAnh3 == null)
                    {
                        hinhAnh.HinhAnh2 = HinhAnh2;
                        var ha = db.BangTins.Where(s => s.ID == bt.ID).FirstOrDefault();
                        bt.HinhAnh1 = ha.HinhAnh1;
                        bt.HinhAnh3 = ha.HinhAnh3;
                        LuuAnhTT(bt, hinhAnh);
                    }
                    else if (HinhAnh1 == null && HinhAnh2 == null)
                    {
                        hinhAnh.HinhAnh3 = HinhAnh3;
                        var ha = db.BangTins.Where(s => s.ID == bt.ID).FirstOrDefault();
                        bt.HinhAnh1 = ha.HinhAnh1;
                        bt.HinhAnh2 = ha.HinhAnh2;
                        LuuAnhTT(bt, hinhAnh);
                    }
                    else if (HinhAnh2 == null && HinhAnh3 == null)
                    {
                        hinhAnh.HinhAnh1 = HinhAnh1;
                        var ha = db.BangTins.Where(s => s.ID == bt.ID).FirstOrDefault();
                        bt.HinhAnh2 = ha.HinhAnh2;
                        bt.HinhAnh3 = ha.HinhAnh3;
                        LuuAnhTT(bt, hinhAnh);
                    }
                    if (user.MaCV == "NV")
                    {
                        var location = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                        db.sp_ChinhSuaTT(bt.ID, bt.TieuDe, bt.NoiDung, bt.HinhAnh1, bt.HinhAnh2, bt.HinhAnh3, location.LocationID);
                    }
                    else
                    {
                        db.sp_ChinhSuaTT(bt.ID, bt.TieuDe, bt.NoiDung, bt.HinhAnh1, bt.HinhAnh2, bt.HinhAnh3, bt.LocationID);
                    }                    
                    db.SaveChanges();
                    TempData["messageAlert"] = "success1";
                    TempData["matt"] = bt.ID;
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error1";
                    TempData["matt"] = bt.ID;
                    TempData["messER"] = e.InnerException.Message;
                }
                return RedirectToAction("DSTT");
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Lấy dữ liệu hình ảnh
        public class ChiTietTT
        {
            public string HinhAnh1 { get; set; }
            public string HinhAnh2 { get; set; }
            public string HinhAnh3 { get; set; }
        }
        [HttpPost]
        public JsonResult GetDataTT(string MaTT)
        {
            try
            {
                var dstt = db.BangTins.Where(s => s.ID == MaTT).FirstOrDefault();
                var result = new ChiTietTT
                {
                    HinhAnh1 = dstt.HinhAnh1,
                    HinhAnh2 = dstt.HinhAnh2,
                    HinhAnh3 = dstt.HinhAnh3
                };
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //Thu thồi tin tức
        [HttpPost]
        public ActionResult ThuHoiTT(string MaTT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockTT(MaTT, false);
                    db.SaveChanges();
                    return Json(new { success = true, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi tin tức
        [HttpPost]
        public ActionResult PhucHoiTT(string MaTT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockTT(MaTT, true);
                    db.SaveChanges();
                    return Json(new { success = true, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xóa tin tức
        [HttpPost]
        public ActionResult XoaTT(string MaTT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaTT(MaTT);
                    db.SaveChanges();
                    return Json(new { success = true, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, matt = MaTT }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        // Danh sách khuyến mãi ở chi nhánh
        public ActionResult DSKM(string id)
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {
                var cn = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var dsmacn = db.Vouchers.Where(s => s.LocationID == cn.LocationID).ToList();
                return View(dsmacn);
            }


            if (id != null)
            {
                var dsmacn = db.Vouchers.Where(s => s.LocationID == id).ToList();
                if (dsmacn != null)
                {
                    return View(dsmacn);
                }
                else
                {
                    return View();
                }
            }
            return View();
        }
        //Lấy thông tin hiển thị cho voucher chi nhánh
        //public class VoucherCN
        //{
        //    public string ID { get; set; }
        //    public string TenVC { get; set; }
        //    public DateTime NgayApDung { get; set; }
        //    public DateTime NgayHetHan { get; set; }
        //    public string MoTa { get; set; }
        //    public double Discount { get; set; }
        //}
        [HttpPost]
        public JsonResult getDataVoucherCN(string MaCN, bool Lock01)
        {
            var dsVoucher = db.Vouchers.Where(s => s.LocationID == MaCN && s.Lock == Lock01).ToList();
            List<Voucher> result = dsVoucher.Select(ma => new Voucher
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
            if (dsVoucher != null)
            {
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Khong co du lieu tim thay" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult KTVoucherCN(string TenVoucher, string MaCN)
        {
            var check = db.Vouchers.Where(s => s.TenVoucher == TenVoucher && s.LocationID == MaCN).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Thêm voucher cho chi nhánh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemVoucherCN([Bind(Include = "TenVoucher, NgayApDung, NgayHetHan, MoTa, Discount,LocationID,ConLai")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = (NhanVien)Session["userNV"];
                    db.sp_ThemVoucher(voucher.TenVoucher, voucher.NgayApDung, voucher.NgayHetHan, voucher.MoTa, voucher.Discount / 100, voucher.LocationID,true,voucher.ConLai,user.MaNV);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["voucherCN"] = voucher.LocationID;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["voucherCN"] = voucher.LocationID;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSKM", new { id = voucher.LocationID });
            }
            return HttpNotFound();
        }
        //Chỉnh sửa voucher cho chi nhánh
        //Chỉnh sửa Voucher
        [HttpPost]
        public ActionResult ChinhSuaVoucherCN([Bind(Include = "ID, TenVoucher, NgayApDung, NgayHetHan, MoTa, Discount, Lock")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                var vc = db.Vouchers.Where(s => s.ID == voucher.ID).FirstOrDefault();
                try
                {

                    db.sp_ChinhSuaVoucher(voucher.ID, voucher.TenVoucher, vc.NgayApDung, vc.NgayHetHan, voucher.MoTa, voucher.Discount / 100, voucher.Lock);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success1";
                    TempData["mavoucher"] = vc.LocationID;
                    return Json(new { success = true, MaCN = vc.LocationID }, JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("DSKM", new { id = voucher.LocationID });
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error1";
                    TempData["mavoucher"] = voucher.LocationID;
                    TempData["mess"] = e.InnerException.Message;
                    return Json(new { success = false, MaCN = voucher.LocationID }, JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("DSKM", new { id = voucher.LocationID });
                }
            }
            return HttpNotFound();
        }
        //Thu hồi Voucher chi nhánh
        [HttpPost]
        public ActionResult ThuHoiVoucherCN(string IDVoucher)
        {
            if (ModelState.IsValid)
            {
                var vc = db.Vouchers.Where(s => s.ID == IDVoucher).FirstOrDefault();
                try
                {

                    TempData["messageAlert"] = "success2";
                    TempData["mavoucher"] = vc.LocationID;
                    db.sp_LockVoucher(IDVoucher);
                    db.SaveChanges();
                    return Json(new { success = true, MaCN = vc.LocationID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error2";
                    TempData["mavoucher"] = vc.LocationID;
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi Voucher chi nhánh
        [HttpPost]
        public ActionResult PhucHoiVoucherCN(string IDVoucher)
        {
            if (ModelState.IsValid)
            {
                var vc = db.Vouchers.Where(s => s.ID == IDVoucher).FirstOrDefault();
                try
                {
                    TempData["messageAlert"] = "success3";
                    TempData["mavoucher"] = vc.LocationID;
                    db.sp_LockVoucher(IDVoucher);
                    db.SaveChanges();
                    return Json(new { success = true, MaCN = vc.LocationID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xoá Voucher chi nhánh
        [HttpPost]
        public ActionResult XoaVoucherCN(string IDVoucher)
        {
            if (ModelState.IsValid)
            {
                var vc = db.Vouchers.Where(s => s.ID == IDVoucher).FirstOrDefault();
                try
                {
                    TempData["messageAlert"] = "success4";
                    TempData["mavoucher"] = vc.LocationID;
                    db.sp_XoaVoucher(IDVoucher);
                    db.SaveChanges();
                    return Json(new { success = true, MaCN = vc.LocationID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Xét duyệt voucher chi nhánh
        [HttpPost]
        public ActionResult DuyetVoucherCN(string MaVC)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetVoucher(MaVC, 1);
                    db.SaveChanges();
                    return Json(new { success = true, mavc = MaVC }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mavc = MaVC }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Từ chối voucher chi nhánh
        [HttpPost]
        public ActionResult TuchoiVoucherCN(string MaVC)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XetDuyetVoucher(MaVC, 2);
                    db.SaveChanges();
                    return Json(new { success = true, mavc = MaVC }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, mavc = MaVC }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }






        //Thảo
        public ActionResult DSNhanVien()
        {
            var dsnv = db.NhanViens.ToList();
            return View(dsnv);
        }

        //Thêm nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemNV([Bind(Include = "UserName,Password,TenNV,SDT,DiaChi,Email,HinhAnh,NgaySinh,GioiTinh,MaCV ")] NhanVien nv, HttpPostedFileBase AvatarFile)
        {
            if (ModelState.IsValid)
            {
                try
                {//Thêm ảnh
                    if (AvatarFile != null && AvatarFile.ContentLength > 0)
                    {
                        // Đường dẫn tới thư mục chứa ảnh đại diện
                        string folderPath = Server.MapPath("~/Content/Avatar/");

                        // Tạo tên file duy nhất
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);

                        // Đường dẫn tới file avatar
                        string filePath = Path.Combine(folderPath, uniqueFileName);

                        // Lưu ảnh lên server
                        AvatarFile.SaveAs(filePath);

                        // Cập nhật link hình trên database
                        nv.HinhAnh = "~/Content/Avatar/" + uniqueFileName;
                    }
                    db.sp_ThemNV(nv.MaNV, nv.UserName, nv.Password, nv.TenNV, nv.SDT, nv.DiaChi, nv.Email, nv.HinhAnh, nv.NgaySinh, nv.GioiTinh, nv.MaCV, true);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tennv"] = nv.TenNV;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tennv"] = nv.TenNV;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSNhanVien");
            }
            return HttpNotFound();
        }

        //Kiểm tra Username
        [HttpPost]
        public ActionResult KTUserNameNV(string UserName)
        {
            var check = db.NhanViens.Where(s => s.UserName == UserName).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //Chỉnh sửa nhân viên
        [HttpPost]
        public ActionResult ChinhSuaNV([Bind(Include = "MaNV,UserName,Password,TenNV,SDT,DiaChi,Email,HinhAnh,NgaySinh,GioiTinh,MaCV")] NhanVien nv)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_SuaThongTinNV(nv.MaNV, nv.Password, nv.TenNV, nv.SDT, nv.DiaChi, nv.Email, nv.HinhAnh, nv.NgaySinh, nv.GioiTinh, nv.MaCV);
                    db.SaveChanges();
                    return Json(new { success = true, manv = nv.MaNV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maNV = nv.MaNV }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Thu hồi nhân viên
        [HttpPost]
        public ActionResult ThuHoiNV(string MaNV)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockNV(MaNV);
                    db.SaveChanges();
                    return Json(new { success = true, manv = MaNV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, makh = MaNV }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Phục hồi nhân viên
        [HttpPost]
        public ActionResult PhucHoiNV(string MaNV)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockNV(MaNV);
                    db.SaveChanges();
                    return Json(new { success = true, manv = MaNV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, makh = MaNV }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Xoá NV
        [HttpPost]
        public ActionResult XoaNV(string MaNV)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaNV(MaNV);
                    db.SaveChanges();
                    return Json(new { success = true, manv = MaNV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, manv = MaNV }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DSRoles()
        {
            var roles = db.ChucVus.ToList();
            return View(roles);
        }

        //Thêm chức vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemCV([Bind(Include = "MaCV,TenCV,ViTri")] ChucVu cv)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemCV(cv.MaCV, cv.TenCV, cv.ViTri, true);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tennv"] = cv.TenCV;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tennv"] = cv.TenCV;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSRoles");
            }
            return HttpNotFound();
        }

        //Kiểm tra Username
        [HttpPost]
        public ActionResult KTUserNameCV(string UserName)
        {
            var check = db.NhanViens.Where(s => s.UserName == UserName).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //Chỉnh sửa chức vụ
        [HttpPost]
        public ActionResult ChinhSuaCV([Bind(Include = "MaCV, TenCV, ViTri ")] ChucVu cv)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaCV(cv.MaCV, cv.TenCV, cv.ViTri);
                    db.SaveChanges();
                    return Json(new { success = true, macv = cv.MaCV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maCV = cv.MaCV }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Thu hồi chức vụ
        [HttpPost]
        public ActionResult ThuHoiCV(string MaCV)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockCV(MaCV);
                    db.SaveChanges();
                    return Json(new { success = true, macv = MaCV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, macv = MaCV }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Phục hồi chức vụ
        [HttpPost]
        public ActionResult PhucHoiCV(string MaCV)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockCV(MaCV);
                    db.SaveChanges();
                    return Json(new { success = true, macv = MaCV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, macv = MaCV }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Xoá CV
        [HttpPost]
        public ActionResult XoaCV(string MaCV)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaCV(MaCV);
                    db.SaveChanges();
                    return Json(new { success = true, macv = MaCV }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, manv = MaCV }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Ngân

        //Khách hàng

        public ActionResult DSKH()
        {
            var dskh = db.KhachHangs.ToList();
            return View(dskh);
        }
        //Thêm khách hàng
        [HttpPost] //Tại sao dùng POST không dùng GET
        [ValidateAntiForgeryToken]
        public ActionResult ThemKH([Bind(Include = "UserName,Password,TenKH,SDT,DiaChi,Email,HinhAnh,NgaySinh,GioiTinh,IDLoaiKH ")] KhachHang kh) //[Bind(Include = "UserName,Password,TenNV")] để làm gì?
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemKH(kh.UserName, kh.Password, kh.TenKH, kh.SDT, kh.DiaChi, kh.Email, kh.HinhAnh, kh.NgaySinh, kh.GioiTinh, kh.IDLoaiKH, true);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tenkh"] = kh.TenKH;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tenkh"] = kh.TenKH;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSKH");
            }
            return HttpNotFound();
        }
        //Kiểm tra Username
        [HttpPost]
        public ActionResult KTUserName(string UserName)
        {
            var check = db.KhachHangs.Where(s => s.UserName == UserName).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //Chỉnh sửa Khách hàng
        [HttpPost]
        public ActionResult ChinhSuaKH([Bind(Include = "MaKH, UserName,Password,TenKH,SDT,DiaChi,Email,HinhAnh,NgaySinh,GioiTinh,IDLoaiKH ")] KhachHang kh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_SuaThongTinKH(kh.MaKH, kh.Password, kh.TenKH, kh.SDT, kh.DiaChi, kh.Email, kh.HinhAnh, kh.NgaySinh, kh.GioiTinh, kh.IDLoaiKH);
                    db.SaveChanges();
                    return Json(new { success = true, maKH = kh.MaKH }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maKH = kh.MaKH }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Thu hồi khách hàng
        [HttpPost]
        public ActionResult ThuHoiKH(string MaKH)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockKH(MaKH);
                    db.SaveChanges();
                    return Json(new { success = true, makh = MaKH }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, makh = MaKH }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Phục hồi khách hàng
        [HttpPost]
        public ActionResult PhucHoiKH(string MaKH)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockKH(MaKH);
                    db.SaveChanges();
                    return Json(new { success = true, makh = MaKH }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, makh = MaKH }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        //Xoá KH
        [HttpPost]
        public ActionResult XoaKH(string MaKH)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaKH(MaKH);
                    db.SaveChanges();
                    return Json(new { success = true, makh = MaKH }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, makh = MaKH }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }





        // Loại khách hàng

        //Hiển thị danh sách loại KH
        public ActionResult DSLKH()
        {
            var dslkh = db.LoaiKHs.ToList();
            return View(dslkh);
        }
        //Thêm loại khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemLKH([Bind(Include = "LoaiKH1, ChietKhau")] LoaiKH lkh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemLoaiKH(lkh.LoaiKH1, lkh.ChietKhau, true);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tenlkh"] = lkh.LoaiKH1;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tenlkh"] = lkh.LoaiKH1;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSLKH");
            }
            return HttpNotFound();
        }

        //Kiểm tra tên LoaiKH
        [HttpPost]
        public ActionResult KTLKH(string LoaiKH1)
        {
            var check = db.LoaiKHs.Where(s => s.LoaiKH1 == LoaiKH1).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        //Chỉnh sửa loại Khách hàng
        [HttpPost]
        public ActionResult ChinhSuaLKH([Bind(Include = "ID, LoaiKH1, ChietKhau")] LoaiKH lkh)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaLoaiKH(lkh.ID, lkh.LoaiKH1, lkh.ChietKhau);
                    db.SaveChanges();
                    return Json(new { success = true, maLKH = lkh.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maLKH = lkh.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        //Xoá LKH
        [HttpPost]
        public ActionResult XoaLKH(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaLoaiKH(ID);
                    db.SaveChanges();
                    return Json(new { success = true, malkh = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, malkh = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Thu hồi loại khách hàng
        [HttpPost]
        public ActionResult ThuHoiLKH(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockLoaiKH(ID);
                    db.SaveChanges();
                    return Json(new { success = true, malkh = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, malkh = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Phục hồi loại khách hàng
        [HttpPost]
        public ActionResult PhucHoiLKH(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockLoaiKH(ID);
                    db.SaveChanges();
                    return Json(new { success = true, malkh = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, malkh = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        // Voucher
        public ActionResult DSVoucher()
        {
            var dsvoucher = db.Vouchers.ToList();
            return View(dsvoucher);
        }
        //Thêm Voucher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemVoucher([Bind(Include = "TenVoucher, NgayApDung, NgayHetHan, MoTa, Discount,LocationID,ConLai")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = (NhanVien)Session["userNV"];
                    db.sp_ThemVoucher(voucher.TenVoucher, voucher.NgayApDung, voucher.NgayHetHan, voucher.MoTa, voucher.Discount / 100, voucher.LocationID,true,voucher.ConLai,user.MaNV);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";
                    TempData["tenvoucher"] = voucher.TenVoucher;

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["tenvoucher"] = voucher.TenVoucher;
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSVoucher");
            }
            return HttpNotFound();
        }
        //Kiểm tra Voucher
        [HttpPost]
        public ActionResult KTVoucher(string TenVoucher)
        {
            var check = db.Vouchers.Where(s => s.TenVoucher == TenVoucher).FirstOrDefault();
            if (check is null)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //Chỉnh sửa Voucher
        [HttpPost]
        public ActionResult ChinhSuaVoucher([Bind(Include = "ID, TenVoucher, NgayApDung, NgayHetHan, MoTa, Discount, Lock")] Voucher voucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ChinhSuaVoucher(voucher.ID, voucher.TenVoucher, voucher.NgayApDung, voucher.NgayHetHan, voucher.MoTa, voucher.Discount, voucher.Lock);
                    db.SaveChanges();
                    return Json(new { success = true, maVoucher = voucher.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = voucher.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Thu hồi Voucher
        [HttpPost]
        public ActionResult ThuHoiVoucher(string IDVoucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockVoucher(IDVoucher);
                    db.SaveChanges();
                    return Json(new { success = true, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Phục hồi Voucher
        [HttpPost]
        public ActionResult PhucHoiVoucher(string IDVoucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockVoucher(IDVoucher);
                    db.SaveChanges();
                    return Json(new { success = true, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        //Xoá Voucher
        [HttpPost]
        public ActionResult XoaVoucher(string IDVoucher)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaVoucher(IDVoucher);
                    db.SaveChanges();
                    return Json(new { success = true, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maVoucher = IDVoucher }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Feedback
        //Hiển thị feedback
        public ActionResult DSFeedback()
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {
                var cn = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var dshoadon = db.HoaDons.Where(s => s.LocationID == cn.LocationID)
                    .Select(s=>s.ID).ToList();
                var dsfeedback = db.Feedbacks.Where(s=> dshoadon.Contains(s.IDHoaDon))
                    .ToList();
                return View(dsfeedback);
            }
            else
            {
                var dsfeedback = db.Feedbacks.ToList();
                return View(dsfeedback);
            }
            
        }
        //Thêm Feedback
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemFeedback([Bind(Include = "NgayTao, DanhGia, NoiDung, IDHoaDon")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_ThemFeedback(feedback.NgayTao, feedback.DanhGia, feedback.NoiDung, feedback.IDHoaDon, true);
                    db.SaveChanges();
                    TempData["messageAlert"] = "success0";

                }
                catch (Exception e)
                {
                    TempData["messageAlert"] = "error0";
                    TempData["mess"] = e.InnerException.Message;
                }
                return RedirectToAction("DSFeedback");
            }
            return HttpNotFound();
        }

        //Chỉnh sửa Feedback
        [HttpPost]
        public ActionResult ChinhSuaFeedback([Bind(Include = "ID, DanhGia, NoiDung")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_SuaFeedback(feedback.ID, feedback.DanhGia, feedback.NoiDung);
                    db.SaveChanges();
                    return Json(new { success = true, maFeedback = feedback.ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maFeedback = feedback.ID }, JsonRequestBehavior.AllowGet);
                }

            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Xoá Feedback
        [HttpPost]
        public ActionResult XoaFeedback(string IDFeedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_XoaFeedback(IDFeedback);
                    db.SaveChanges();
                    return Json(new { success = true, maFeedback = IDFeedback }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maFeedback = IDFeedback }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Thu hồi Feedback
        [HttpPost]
        public ActionResult ThuHoiFeedback(string IDFeedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockFeedback(IDFeedback);
                    db.SaveChanges();
                    return Json(new { success = true, maFeedback = IDFeedback }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maFeedback = IDFeedback }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Phục hồi Feedback
        [HttpPost]
        public ActionResult PhucHoiFeedback(string IDFeedback)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockFeedback(IDFeedback);
                    db.SaveChanges();
                    return Json(new { success = true, maFeedback = IDFeedback }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maFeedback = IDFeedback }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Hoá đơn
        //Hiển thị hoá đơn
        public ActionResult DSHD()
        {
            var user = (NhanVien)Session["userNV"];
            if (user.MaCV == "NV")
            {
                var cn = db.ChiNhanhs.Where(s => s.NVQL == user.MaNV).FirstOrDefault();
                var dshdcn = db.HoaDons.Where(s => s.LocationID == cn.LocationID).ToList();
                return View(dshdcn);
            }

            var dshd = db.HoaDons.ToList();
            return View(dshd);
        }
        //Xét duyệt hóa đơn
        [HttpPost]
        public ActionResult XetDuyetHD(string MaHD, bool TT)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (TT == true)
                    {
                        var check = db.HoaDons.Where(s => s.ID == MaHD).FirstOrDefault();
                        var ttkh = db.KhachHangs.Where(s=>s.MaKH == check.MaKH).FirstOrDefault();
                        if(ttkh != null)
                        {
                            KhachHangController ck = new KhachHangController();
                            string tongtien = String.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:C}", check.TongCong);
                            ck.sendEmail(ttkh,tongtien,MaHD);
                        }
                        db.sp_CheckTonKho(MaHD, check.LocationID);
                        db.sp_XetDuyetHD(MaHD, TT);
                    }
                    else
                    {
                        var check = db.HoaDons.Where(s => s.ID == MaHD).FirstOrDefault();
                        var ttkh = db.KhachHangs.Where(s => s.MaKH == check.MaKH).FirstOrDefault();
                        if (ttkh != null)
                        {
                            KhachHangController ck = new KhachHangController();
                            string tongtien = String.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:C}", check.TongCong);
                            ck.sendEFaileOrder(ttkh,tongtien,MaHD);
                        }
                        db.sp_XetDuyetHD(MaHD, TT);
                    }
                    db.SaveChanges();
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        //Thu hồi Hoá đơn
        [HttpPost]
        public ActionResult ThuHoiHD(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockHD(ID);
                    db.SaveChanges();
                    return Json(new { success = true, maHD = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maHD = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }
        //Phục hồi Hoá đơn
        [HttpPost]
        public ActionResult PhucHoiHD(string ID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.sp_LockHD(ID);
                    db.SaveChanges();
                    return Json(new { success = true, maHD = ID }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, nd = e.InnerException.Message, maHD = ID }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        //Chi tiết hoá đơn
        //Hàm lấy thông tin chi tiết hóa đơn
        public class chitietHoaDon
        {
            public string ImageMA { get; set; }
            public string TenMonAn { get; set; }
            public int SoLuong { get; set; }
            public double Gia { get; set; }
        }
        [HttpPost]
        public JsonResult getChiTietHD(string MaHD)
        {
            var dsCTHoaDon = db.ChiTietHDs.Where(s => s.IDHoaDon == MaHD).ToList();
            var mavoucher = db.HoaDons.Where(s => s.ID == MaHD).FirstOrDefault();
            var discout = db.Vouchers.Where(s => s.ID == mavoucher.IDVoucher).FirstOrDefault();

            List<chitietHoaDon> result = dsCTHoaDon.Select(ma => new chitietHoaDon
            {
                ImageMA = ma.MonAn.HinhAnh,
                TenMonAn = ma.MonAn.TenMA,
                SoLuong = (int)ma.SoLuong,
                Gia = (double)ma.GiaBan
            }).ToList();
            if (dsCTHoaDon != null && discout != null)
            {
                return Json(new { success = true, data = result, vouCher = discout.Discount, checkTT = mavoucher.TrangThai }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = true, data = result, vouCher = 0, checkTT = mavoucher.TrangThai }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DSBH()
        {
            return View();
        }

        [HttpPost]
        public JsonResult getDoanhThu(int year, string branch)
        {
            if(branch != "all")
            {
                var dsHoaDon = db.HoaDons.Where(hd => hd.NgayTao.Value.Year == year && hd.LocationID == branch && hd.TrangThai == true).ToList();
                List<HoaDon> result = dsHoaDon.Select(ma => new HoaDon
                {
                    ID = ma.ID,
                    NgayTao = ma.NgayTao,
                    LocationID = ma.LocationID,
                    TongCong = ma.TongCong,
                    IDVoucher = ma.IDVoucher,
                    MaNVLap = ma.MaNVLap,
                    MaKH = ma.MaKH,
                    TrangThai = ma.TrangThai
                }).ToList();

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var dsHoaDon = db.HoaDons.Where(hd => hd.NgayTao.Value.Year == year && hd.TrangThai == true).ToList();
                List<HoaDon> result = dsHoaDon.Select(ma => new HoaDon
                {
                    ID = ma.ID,
                    NgayTao = ma.NgayTao,
                    LocationID = ma.LocationID,
                    TongCong = ma.TongCong,
                    IDVoucher = ma.IDVoucher,
                    MaNVLap = ma.MaNVLap,
                    MaKH = ma.MaKH,
                    TrangThai = ma.TrangThai
                }).ToList();
                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}