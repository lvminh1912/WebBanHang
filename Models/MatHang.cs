using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("MatHang")]
    public class MatHang
    {
        [Key]
        public int MaMatHang { get; set; }

        [Required]
        [StringLength(150)]
        public string TenMatHang { get; set; } = null!;

        [Required]
        public int MaLoaiHang { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaBan { get; set; }

        [StringLength(50)]
        public string? DonViTinh { get; set; }

        [StringLength(255)]
        public string? HinhAnh { get; set; }

        public string? MoTa { get; set; }

        public int? SoLuong { get; set; }

        public bool DangBan { get; set; } = true;

        // Navigation Properties
        [ForeignKey("MaLoaiHang")]
        public virtual LoaiHang? LoaiHang { get; set; }

        public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();
        public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; } = new List<GioHangChiTiet>();
    }
}