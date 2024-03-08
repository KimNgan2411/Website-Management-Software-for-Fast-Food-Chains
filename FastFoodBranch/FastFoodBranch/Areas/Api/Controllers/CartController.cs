using FastFoodBranch.App_Start;
using FastFoodBranch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebGrease.Activities;
using static FastFoodBranch.Controllers.KhachHangController;

namespace FastFoodBranch.Areas.Api.Controllers
{
    [Throttle(limit: 5, periodInSeconds: 60)] // Giới hạn 5 yêu cầu trong 1 phút
    public class CartController : Controller
    {
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        //API thông tin của giỏ hàng
        //MB : mobile được bổ sung vào đầu mỗi action Result
        // GET: Api lấy số lượng món ăn đang có trong giỏ hàng      
        //public ActionResult MBLaySLCart()
        //{
        //    int sl = 0;
        //    Cart cart = Session["MBCart"] as Cart;
        //    if (cart == null)
        //    {
        //        sl = 0;
        //    }
        //    else
        //    {
        //        sl = cart.TongSLTrongGio();
        //    }
        //    return Json(new { success = true, sl = sl }, JsonRequestBehavior.AllowGet);
        //}
        //Api sẽ lấy dữ liệu thông tin khách hàng (vãng lai || đã login) từ dưới db
        public ActionResult MBLaySLCart(string idkh)
        {
            try
            {
                getDataCart(idkh);
                int sl = 0;
                sl = GetCart().TongSLTrongGio();
                if (sl != 0)
                {
                    var macn = (string)Session["MBlocationCN"];
                    var idSession = Session.SessionID;
                    var data = new
                    {
                        KQ = 1,
                        maCN = macn,
                        sl = sl,
                        SessionID = idSession
                    };
                    return Json(new { success = true, data = data  }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = new
                    {
                        KQ = 0,
                        sl = sl
                    };
                    return Json(new { success = false, data = data }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.InnerException.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        //Thêm món ăn vào giỏ hàng
        [HttpPost]
        public ActionResult MBAddCart(string id, int sl, string idkh)
        {
            try
            {
                var sp = db.MonAns.SingleOrDefault(s => s.ID == id);
                var data = new { sl = 0 };
                if (sp != null)
                {
                    GetCart().Add(sp, sl);
                    var slht = GetCart().TongSLTrongGio();
                    //Kiểm tra người dùng có đăng nhập hay không
                    addCartDB(id, sl, idkh);
                    data = new
                    {
                        sl = slht
                    };
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var slht = GetCart().TongSLTrongGio();
                    data = new
                    {
                        sl = slht
                    };
                    return Json(new { success = false, data = data }, JsonRequestBehavior.AllowGet);
                }
            }catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }

        //Api sẽ lấy danh sách toàn bộ thông tin món ăn có trong giỏ hàng
        [AuthenticationFilter]
        public ActionResult MBDSCart()
        {
            try
            {
                List<CartDataModel> result = new List<CartDataModel>();
                var data = GetCart().Items.ToList();
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        if (item != null)
                        {
                            result.Add(new CartDataModel
                            {
                                ID = item.idMA.ID,
                                TenMA = item.idMA.TenMA,
                                MoTa = item.idMA.MoTa,
                                GiaGoc = item.idMA.GiaGoc,
                                HinhAnh = item.idMA.HinhAnh,
                                soLuong = item.soLuong
                            });
                        }
                    }
                }
                return Json(new { success = true, data = result}, JsonRequestBehavior.AllowGet);
            }catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }
        //Kiểm tra mã khuyến mãi
        [HttpPost]
        public ActionResult MBApDungVC(string id)
        {
            //Kết quả success trả về thất bại khi == 0 , 2
            try
            {
                double tongcong = 0;
                var data = new
                {
                    vc = 0.0,
                    tc = 0.0
                };
                if (id != null)
                {
                    var check = db.Vouchers.Where(s => s.ID == id && s.ConLai > 0).FirstOrDefault();
                    var location = (string)Session["MBlocationCN"];

                    if (check == null)
                    {
                        GetCart().setmaVC(null);
                        //var tongcong = GetCart().TongTien() - (GetCart().TongTien() * check.Discount);
                        tongcong = GetCart().TongTien();
                        data = new
                        {
                            vc = 0.0,
                            tc = tongcong
                        };
                        return Json(new { success = 0, data = data }, JsonRequestBehavior.AllowGet);
                    }
                    else if (check != null)
                    {
                        //kiểm tra mã voucher này có phù hợp với chi nhánh này không

                        if (check.LocationID == null) // Mã dành cho toàn hệ thống
                        {
                            GetCart().setmaVC(id);
                            //var tongcong = GetCart().TongTien() - (GetCart().TongTien() * check.Discount);
                            tongcong = GetCart().TongTien();
                            data = new
                            {
                                vc = (double)check.Discount,
                                tc = tongcong
                            };
                            return Json(new { success = 1, data = data }, JsonRequestBehavior.AllowGet);
                        }
                        else if (check.LocationID == location)
                        {
                            //Mã chỉ áp dụng cho từng chi nhánh
                            GetCart().setmaVC(id);
                            //var tongcong = GetCart().TongTien() - (GetCart().TongTien() * check.Discount);
                            tongcong = GetCart().TongTien();
                            data = new
                            {
                                vc = (double)check.Discount,
                                tc = tongcong
                            };
                            return Json(new { success = 1, data = data }, JsonRequestBehavior.AllowGet);
                        }
                        else if (check.LocationID != location)
                        {
                            //Mã này không có có ở chi nhánh này
                            GetCart().setmaVC(null);
                            tongcong = GetCart().TongTien();
                            data = new
                            {
                                vc = (double)0,
                                tc = (double)tongcong
                            };
                            return Json(new { success = 2, data = data }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    //else
                    //{
                    //    GetCart().setmaVC(null);
                    //    tongcong = GetCart().TongTien();
                    //    return Json(new { success = 0, tc = tongcong }, JsonRequestBehavior.AllowGet);
                    //}
                }
                GetCart().setmaVC(null);
                tongcong = GetCart().TongTien();
                return Json(new { success = 0, data = data ,error = "Vui lòng nhập mã!" }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = 0, error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }

        //Xóa sản phẩm ra khỏi giỏ hàng
        [HttpPut]
        public ActionResult MBXoaMA(string id)
        {
            try
            {
                GetCart().XoaSP(id);
                var tamtinh = GetCart().TongTienBefore();
                var tongcong = GetCart().TongTien();
                var sl = GetCart().TongSLTrongGio();
                deleteItemCartDB(id);
                var data = new
                {
                    tt = tamtinh,
                    tc = tongcong,
                    sl = sl
                };
                return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //Cập nhật số lượng 
        [HttpPut]
        public ActionResult MBCapNhatGH(string id, int sl,string idkh)
        {
            if (id != null && sl > 0)
            {
                try
                {
                    GetCart().CapNhatSL(id, sl);
                    var slht = GetCart().TongSLTrongGio();
                    var tamtinh = GetCart().TongTienBefore();
                    var tongcong = GetCart().TongTien();
                    addCartDB(id, sl, idkh);
                    var data = new
                    {
                        sl = slht,
                        tt = tamtinh,
                        tc = tongcong
                    };
                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
                
            }
            else
            {
                return Json(new { success = false, error = "Không thỏa điều kiện đầu vào! Vui lòng kiểm tra lại" }, JsonRequestBehavior.AllowGet);
            }
        }


        public class CartDataModel
        {
            public string ID { get; set; }
            public string TenMA { get; set; }
            public string MoTa { get; set; }
            public Nullable<double> GiaGoc { get; set; }
            public string HinhAnh { get; set; }
            public int soLuong { get; set; }
        }

        
        // Tạo mới giỏ hàng
        public Cart GetCart()
        {
            Cart cart = Session["MBCart"] as Cart;
            if (cart == null || Session["MBCart"] == null)
            {
                cart = new Cart();
                Session["MBCart"] = cart;
            }
            return cart;
        }
        //Thêm món ăn vào giỏ hàng
        public void addCartDB(string id, int sl, string idkh)
        {
            try
            {
                var user = (KhachHang)Session["MBuserKH"];
                var location = Session["MBlocationCN"];
                if (user != null)
                {
                    
                    //Kiểm tra trong giỏ hàng database có tồn tại hay chưa
                    var cartdb = db.ShoppingCarts.Where(s => s.MaKH == user.MaKH && s.IDMonAn == id).FirstOrDefault();
                    if (cartdb != null)
                    {
                        updateCartDB(user.MaKH, id, (string)location);
                    }

                    db.sp_AddCart(user.MaKH, (string)location, id, sl);
                }
                else
                {
                    //Kiểm tra trong giỏ hàng database có tồn tại hay chưa
                    var cartdb = db.ShoppingCarts.Where(s => s.MaKH == idkh && s.IDMonAn == id).FirstOrDefault();
                    if (cartdb != null)
                    {
                        updateCartDB(idkh, id, (string)location);
                    }

                    db.sp_AddCart(idkh, (string)location, id, sl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
            }

        }

        //Cập nhật món ăn vào giỏ hàng
        public void updateCartDB(string makh, string id, string location)
        {
            //Kiểm tra trong giỏ hàng database có tồn tại hay chưa
            try
            {
                var slitem = GetCart().Items.Where(s => s.idMA.ID == id).FirstOrDefault();
                db.sp_UpdateCart(makh, location, id, slitem.soLuong);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
            }

        }
        //Xóa 1 món ăn ra khỏi giỏ hàng 
        public void deleteItemCartDB(string id)
        {
            try
            {
                var user = (KhachHang)Session["MBuserKH"];
                if (user != null)
                {
                    var location = Session["MBlocationCN"];
                    //Kiểm tra trong giỏ hàng database có tồn tại hay chưa
                    var cartdb = db.ShoppingCarts.Where(s => s.MaKH == user.MaKH && s.IDMonAn == id).FirstOrDefault();
                    if (cartdb != null)
                    {
                        db.sp_DeleteItemCart(user.MaKH, id);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
            }
        }

        //Xóa mã kh ra khỏi shopping cart
        public void deleteCartDB()
        {
            try
            {
                var user = (KhachHang)Session["MBuserKH"];
                if (user != null)
                {
                    db.sp_DeleteCart(user.MaKH);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Loi: " + ex.Message);
            }
        }
        //Setup khi người dùng đăng nhập lại sẽ lấy thông tin shopping cart ở db đẩy thông tin lên
        public void getDataCart(string idkh)
        {
            try
            {
                var user = (KhachHang)Session["MBuserKH"];
                if (user != null)
                {
                    GetCart().XoaSauKhiDat();

                    var dsShoppingCart = db.ShoppingCarts.Where(s => s.MaKH == user.MaKH).ToList();
                    if (dsShoppingCart.Count != 0)
                    {
                        var macn = "";
                        foreach (var item in dsShoppingCart)
                        {
                            macn = item.LocationID;
                            var ma = db.MonAns.Where(s => s.ID == item.IDMonAn).FirstOrDefault();
                            GetCart().Add(ma, (int)item.SoLuong);

                        }
                        Session["MBlocationCN"] = macn;
                    }
                }
                else
                {
                    GetCart().XoaSauKhiDat();

                    var dsShoppingCart = db.ShoppingCarts.Where(s => s.MaKH == idkh).ToList();
                    if (dsShoppingCart.Count != 0)
                    {
                        var macn = "";
                        foreach (var item in dsShoppingCart)
                        {
                            macn = item.LocationID;
                            var ma = db.MonAns.Where(s => s.ID == item.IDMonAn).FirstOrDefault();
                            GetCart().Add(ma, (int)item.SoLuong);

                        }
                        Session["MBlocationCN"] = macn;
                    }
                }

            }
            catch (Exception ex)
            {
                Session["MBlocationCN"] = null;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
