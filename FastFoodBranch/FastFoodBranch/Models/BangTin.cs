//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FastFoodBranch.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class BangTin
    {
        public string ID { get; set; }
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public string HinhAnh1 { get; set; }
        public string HinhAnh2 { get; set; }
        public string HinhAnh3 { get; set; }
        public string MaNV { get; set; }
        public Nullable<System.DateTime> NgayDang { get; set; }
        public string LocationID { get; set; }
        public Nullable<bool> Lock { get; set; }
        public Nullable<int> TrangThai { get; set; }
    
        public virtual ChiNhanh ChiNhanh { get; set; }
        public virtual NhanVien NhanVien { get; set; }
    }
}
