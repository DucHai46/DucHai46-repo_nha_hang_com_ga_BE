using repo_nha_hang_com_ga_BE.Models.Repositories;
using repo_nha_hang_com_ga_BE.Models.Repositories.Imp;
using repo_nha_hang_com_ga_BE.Repository;
using repo_nha_hang_com_ga_BE.Repository.Imp;

namespace repo_nha_hang_com_ga_BE.Models.Common.Services
{
    public static class MongoDbServiceExtensions
    {
        public static IServiceCollection AddMongoDbServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Cấu hình settings từ appsettings.json
            services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

            // Đăng ký repository
            services.AddSingleton<IDanhMucNguyenLieuRepository, DanhMucNguyenLieuRepository>();
            services.AddSingleton<IDanhMucMonAnRepository, DanhMucMonAnRepository>();
            services.AddSingleton<ILoaiNguyenLieuRepository, LoaiNguyenLieuRepository>();
            services.AddSingleton<IDonViTinhRepository, DonViTinhRepository>();
            services.AddSingleton<INguyenLieuRepository, NguyenLieuRepository>();
            services.AddSingleton<ILoaiMonAnRepository, LoaiMonAnRepository>();
            services.AddSingleton<ICongThucRepository, CongThucRepository>();
            services.AddSingleton<IKhuyenMaiRepository, KhuyenMaiRepository>();
            services.AddSingleton<ILoaiKhuyenMaiRepository, LoaiKhuyenMaiRepository>();
            services.AddSingleton<IMonAnRepository, MonAnRepository>();
            services.AddSingleton<IBanRepository, BanRepository>();
            services.AddSingleton<IComboRepository, ComboRepository>();
            services.AddSingleton<ILoaiBanRepository, LoaiBanRepository>();
            services.AddSingleton<IThucDonRepository, ThucDonRepository>();
            services.AddSingleton<ILoaiTuDoRepository, LoaiTuDoRepository>();
            services.AddSingleton<ITuDoRepository, TuDoRepository>();
            services.AddSingleton<IKhachHangRepository, KhachHangRepository>();
            services.AddSingleton<IMenuDynamicRepository, MenuDynamicRepository>();
            services.AddSingleton<IGiamGiaRepository, GiamGiaRepository>();
            services.AddSingleton<IDonDatBanRepository, DonDatBanRepository>();
            services.AddSingleton<IDonOrderRepository, DonOrderRepository>();
            services.AddSingleton<ILoaiDonRepository, LoaiDonRepository>();
            services.AddSingleton<IPhuongThucThanhToanRepository, PhuongThucThanhToanRepository>();

            return services;
        }
    }
}