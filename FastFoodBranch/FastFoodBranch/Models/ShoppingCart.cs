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
    
    public partial class ShoppingCart
    {
        public string MaKH { get; set; }
        public string LocationID { get; set; }
        public string IDMonAn { get; set; }
        public Nullable<int> SoLuong { get; set; }
    
        public virtual KhachHang KhachHang { get; set; }
    }
}
