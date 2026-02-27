namespace Models.SearchView
{
    public class DashboardViewModel
    {
        public int TongDonHang { get; set; }
        public int TongSanPham { get; set; }
        public int TongKhachHang { get; set; }
        public decimal DoanhThu { get; set; }
        public List<DonHang> DonHangMoiNhat { get; set; } = new List<DonHang>();
    }
}