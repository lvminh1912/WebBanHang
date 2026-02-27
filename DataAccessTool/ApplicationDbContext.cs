using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccessTool
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #region DbSet Entities
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<LoaiHang> LoaiHangs { get; set; }
        public DbSet<MatHang> MatHangs { get; set; }
        public DbSet<TrangThaiDon> TrangThaiDons { get; set; }
        public DbSet<DonHang> DonHangs { get; set; }
        public DbSet<DonHangChiTiet> DonHangChiTiets { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }
        public DbSet<TinhThanh> TinhThanhs { get; set; }
        #endregion

        /// <summary>
        /// Cấu hình quy ước chung cho toàn bộ Database (Dành cho .NET 6, 7, 8+)
        /// </summary>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Tự động cấu hình tất cả kiểu decimal thành decimal(18,2) trong SQL
            configurationBuilder.Properties<decimal?>().HavePrecision(18, 2);
        }

        /// <summary>
        /// Cấu hình chi tiết các ràng buộc và quan hệ
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình bảng KhachHang
            modelBuilder.Entity<KhachHang>(entity => {
                entity.ToTable("KhachHang");
                entity.HasIndex(e => e.Email).IsUnique(); // Email không được trùng
            });

            // 2. Cấu hình bảng TrangThaiDon (Vì không dùng Identity tăng tự động)
            modelBuilder.Entity<TrangThaiDon>(entity => {
                entity.ToTable("TrangThaiDon");
                entity.Property(e => e.MaTrangThai).ValueGeneratedNever();
            });

            // 3. Cấu hình bảng MatHang
            modelBuilder.Entity<MatHang>(entity => {
                entity.ToTable("MatHang");
                entity.Property(e => e.DangBan).HasDefaultValue(true);
            });

            // 4. Cấu hình quan hệ cho DonHangChiTiet (Khóa ngoại)
            modelBuilder.Entity<DonHangChiTiet>(entity => {
                entity.ToTable("DonHangChiTiet");

                entity.HasOne(d => d.DonHang)
                      .WithMany(p => p.DonHangChiTiets)
                      .HasForeignKey(d => d.MaDonHang)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa đơn hàng thì xóa chi tiết

                entity.HasOne(d => d.MatHang)
                      .WithMany(p => p.DonHangChiTiets)
                      .HasForeignKey(d => d.MaMatHang)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa mặt hàng nếu đã có trong đơn hàng
            });

            // 5. Cấu hình các bảng còn lại để khớp tên với SQL
            modelBuilder.Entity<LoaiHang>().ToTable("LoaiHang");
            modelBuilder.Entity<DonHang>().ToTable("DonHang");
            modelBuilder.Entity<GioHang>().ToTable("GioHang");
            modelBuilder.Entity<GioHangChiTiet>().ToTable("GioHangChiTiet");
            modelBuilder.Entity<TinhThanh>().ToTable("TinhThanh");
        }
    }
}