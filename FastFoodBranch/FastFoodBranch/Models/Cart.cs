using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FastFoodBranch.Models
{
    public class CartItem
    {
        public MonAn idMA { get; set; }
        public int soLuong { get; set; }
    }
    public class Cart
    {
        QuanLyFastFoodEntities1 db = new QuanLyFastFoodEntities1();
        public string MaVoucher;
        public double discount;
        List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }

        

        //Thêm sản phẩm vào giỏ hàng
        public void Add(MonAn ma, int sl)
        {
            var item = Items.FirstOrDefault(s => s.idMA.ID == ma.ID);
            if (item == null)
            {
                items.Add(new CartItem
                {
                    idMA = ma,
                    soLuong = sl
                });
            }
            else
            {
                item.soLuong += sl;
            }

        }


        public void setmaVC(string maVC)
        {
            var dc = db.Vouchers.Where(s => s.ID == maVC).FirstOrDefault();
            if(dc != null)
            {
                this.MaVoucher = dc.ID;
                this.discount = (double)dc.Discount;
            }
            else if(dc == null)
            {
                this.MaVoucher = null;
                this.discount = 0;
            }
            

        }

        //Tính tổng số lượng trong giỏ
        public int TongSLTrongGio()
        {
            return items.Sum(s => s.soLuong);
        }

        //Tính thành tiền
        public double ThanhTien(string MaMA)
        {
            var item = items.Find(s => s.idMA.ID == MaMA);
            var thanhTien = item.idMA.GiaGoc * item.soLuong;
            return (double)thanhTien;
        }

        //Tính tổng tiền trước khi áp dụng Voucher
        public double TongTienBefore()
        {
            double tongtien = 0;
            var tong = items.Sum(s => s.soLuong * s.idMA.GiaGoc);
            tongtien = (double)tong;
            return tongtien;
        }

        //Tính tổng tiền
        public double TongTien()
        {
            double tongcong = 0;
            if (this.discount != 0)
            {
                var tong = items.Sum(s => s.soLuong * s.idMA.GiaGoc);
                tongcong = (double)(tong - (tong * discount));

            }
            else
            {
                var tong = items.Sum(s => s.soLuong * s.idMA.GiaGoc);
                tongcong = (double)tong;
            }
            return (double)tongcong;
        }

        //Cập nhật số lượng
        public void CapNhatSL(string id, int slmoi)
        {
            var item = items.Find(s => s.idMA.ID == id);
            if (item != null)
            {
                item.soLuong = slmoi;
            }

        }

        //Xóa sản phẩm trong giỏ hàng
        public void XoaSP(string id)
        {
            items.RemoveAll(s => s.idMA.ID == id);
        }

        //Xóa sản sau khi đặt hàng
        public void XoaSauKhiDat()
        {
            items.Clear();
        }
    }
}