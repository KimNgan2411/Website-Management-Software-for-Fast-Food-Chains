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
    
    public partial class ChiTietKho
    {
        public string ID { get; set; }
        public string IDKho { get; set; }
        public string IDNguyenLieu { get; set; }
        public Nullable<int> SoLuong { get; set; }
    
        public virtual Kho Kho { get; set; }
        public virtual NguyenLieu NguyenLieu { get; set; }
    }
}