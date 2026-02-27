namespace Models.SearchView
{
    public class CheckoutViewModel
    {
        // Thông tin hiển thị (chỉ đọc)
        public string TenKhachHang { get; set; } = "";
        public string SoDienThoai { get; set; } = "";

        // Thông tin người dùng nhập/chỉnh sửa để giao hàng
        public string DiaChiGiaoHang { get; set; } = "";
        public int MaTinh { get; set; }

        // Hiển thị lại danh sách hàng sẽ mua
        public List<GioHangChiTiet>? Items { get; set; }
        public decimal TongTienTamTinh { get; set; }
    }
}