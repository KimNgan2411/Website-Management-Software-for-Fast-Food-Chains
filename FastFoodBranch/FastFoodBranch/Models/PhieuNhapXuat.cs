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
    
    public partial class PhieuNhapXuat
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PhieuNhapXuat()
        {
            this.ChiTietPNXes = new HashSet<ChiTietPNX>();
        }
    
        public string ID { get; set; }
        public string IDKho { get; set; }
        public string IDNV { get; set; }
        public Nullable<bool> LoaiPhieu { get; set; }
        public Nullable<System.DateTime> NgayLap { get; set; }
        public Nullable<bool> Lock { get; set; }
        public Nullable<int> TrangThai { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ChiTietPNX> ChiTietPNXes { get; set; }
        public virtual Kho Kho { get; set; }
    }
}
