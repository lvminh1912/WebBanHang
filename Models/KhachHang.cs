using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    [Table("KhachHang")]
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string HoTen { get; set; } = null!; 

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100)]
        public string MatKhau { get; set; } = null!;

        [StringLength(20)]
        public string? DienThoai { get; set; }

        [StringLength(255)]
        public string? HinhAnh { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public int? MaTinh { get; set; } 

        [ForeignKey("MaTinh")]
        public virtual TinhThanh? TinhThanh { get; set; } 

        [StringLength(5)]
        public string? VaiTro { get; set; }
        public bool DangHoatDong { get; set; } = true;
    }
}