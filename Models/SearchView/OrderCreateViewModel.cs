using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.SearchView
{
    public class OrderCreateViewModel
    {
        // Thông tin chỉ đọc (hiển thị cho khách biết)
        public string TenKhachHang { get; set; } = "";
        public string SoDienThoai { get; set; } = "";

        // Thông tin khách cần nhập/chỉnh sửa
        public string DiaChiGiaoHang { get; set; } = "";
        public int MaTinh { get; set; }

        // Hiển thị lại đơn hàng
        public List<GioHangChiTiet> Items { get; set; } = new List<GioHangChiTiet>();
        public decimal TongTien { get; set; }
    }
}
