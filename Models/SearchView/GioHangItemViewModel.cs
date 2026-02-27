namespace Models.SearchView
{
    public class GioHangItemViewModel
    {
        public int MaMatHang { get; set; }
        public string TenMatHang { get; set; } = string.Empty;
        public string HinhAnh { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }
}
