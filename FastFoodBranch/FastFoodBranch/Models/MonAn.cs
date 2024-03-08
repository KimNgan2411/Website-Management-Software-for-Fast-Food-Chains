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
    
    public partial class MonAn
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MonAn()
        {
            this.BienDongGias = new HashSet<BienDongGia>();
            this.ChiTietHDs = new HashSet<ChiTietHD>();
            this.MonAnChiNhanhs = new HashSet<MonAnChiNhanh>();
        }
    
        public string ID { get; set; }
        public string TenMA { get; set; }
        public string MoTa { get; set; }
        public Nullable<double> GiaGoc { get; set; }
        public string IDDanhMuc { get; set; }
        public string IDCongThuc { get; set; }
        public Nullable<bool> Lock { get; set; }
        public string HinhAnh { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BienDongGia> BienDongGias { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietHD> ChiTietHDs { get; set; }
        public virtual CongThuc CongThuc { get; set; }
        public virtual DanhMuc DanhMuc { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MonAnChiNhanh> MonAnChiNhanhs { get; set; }
    }
}