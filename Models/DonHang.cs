using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("DonHang")]
    public class DonHang
    {
        [Key]
        public int MaDonHang { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        public int? MaTrangThai { get; set; }

        public DateTime? NgayDat { get; set; } = DateTime.Now;

        public DateTime? NgayDi { get; set; }

        public DateTime? NgayDen { get; set; }

        public DateTime? NgayHuy { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TongTien { get; set; }

        [StringLength(255)]
        public string? DiaChiGiaoHang { get; set; }

        // Navigation Properties
        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaTrangThai")]
        public virtual TrangThaiDon? TrangThaiDon { get; set; }

        public virtual ICollection<DonHangChiTiet> DonHangChiTiets { get; set; } = new List<DonHangChiTiet>();

        public int? MaTinh { get; set; }

        [ForeignKey("MaTinh")]
        public virtual TinhThanh? TinhThanh { get; set; }
    }
}