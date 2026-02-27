namespace Models.SearchView
{
    public class GioHangViewModel
    {
        public KhachHang? KhachHang { get; set; }
        public List<GioHangItemViewModel> Items { get; set; } = new List<GioHangItemViewModel>();
        public decimal TongTien => Items.Sum(x => x.ThanhTien);
        public int TongSoLuong => Items.Sum(x => x.SoLuong);
    }
}
