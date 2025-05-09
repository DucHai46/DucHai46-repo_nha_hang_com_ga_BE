using AutoMapper;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.Ban;
using repo_nha_hang_com_ga_BE.Models.Requests.Combo;
using repo_nha_hang_com_ga_BE.Models.Requests.CongThuc;
using repo_nha_hang_com_ga_BE.Models.Requests.DanhMucMonAn;
using repo_nha_hang_com_ga_BE.Models.Requests.DanhMucNguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Requests.DonViTinh;
using repo_nha_hang_com_ga_BE.Models.Requests.KhachHang;
using repo_nha_hang_com_ga_BE.Models.Requests.KhuyenMai;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiBan;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiKhuyenMai;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiMonAn;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiNguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiTuDo;
using repo_nha_hang_com_ga_BE.Models.Requests.MonAn;
using repo_nha_hang_com_ga_BE.Models.Requests.NguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;
using repo_nha_hang_com_ga_BE.Models.Requests.TuDo;
using repo_nha_hang_com_ga_BE.Models.Requests.MenuDynamic;
using repo_nha_hang_com_ga_BE.Models.Requests.GiamGia;
using repo_nha_hang_com_ga_BE.Models.Responds.Ban;
using repo_nha_hang_com_ga_BE.Models.Responds.Combo;
using repo_nha_hang_com_ga_BE.Models.Responds.Common;
using repo_nha_hang_com_ga_BE.Models.Responds.CongThuc;
using repo_nha_hang_com_ga_BE.Models.Responds.DanhMucMonAn;
using repo_nha_hang_com_ga_BE.Models.Responds.DanhMucNguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Responds.DonViTinh;
using repo_nha_hang_com_ga_BE.Models.Responds.KhachHang;
using repo_nha_hang_com_ga_BE.Models.Responds.KhuyenMai;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiBan;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiKhuyenMai;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiMonAn;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiNguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiTuDo;
using repo_nha_hang_com_ga_BE.Models.Responds.MonAn;
using repo_nha_hang_com_ga_BE.Models.Responds.NguyenLieu;
using repo_nha_hang_com_ga_BE.Models.Responds.ThucDon;
using repo_nha_hang_com_ga_BE.Models.Responds.TuDo;
using repo_nha_hang_com_ga_BE.Models.Responds.MenuDynamic;
using repo_nha_hang_com_ga_BE.Models.Responds.GiamGia;
using repo_nha_hang_com_ga_BE.Models.Requests.DonDatBan;
using repo_nha_hang_com_ga_BE.Models.Responds.DonDatBan;
using repo_nha_hang_com_ga_BE.Models.Requests.DonOrder;
using repo_nha_hang_com_ga_BE.Models.Responds.DonOrder;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiDon;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiDon;


namespace repo_nha_hang_com_ga_BE.Models.Common.Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //Danh mục nguyên liệu
        CreateMap(typeof(DanhMucNguyenLieu), typeof(DanhMucNguyenLieuRespond));
        CreateMap(typeof(RequestAddDanhMucNguyenLieu), typeof(DanhMucNguyenLieu));
        CreateMap(typeof(RequestUpdateDanhMucNguyenLieu), typeof(DanhMucNguyenLieu));


        //Loại nguyên liệu
        // CreateMap(typeof(LoaiNguyenLieu), typeof(LoaiNguyenLieuRespond));
        CreateMap(typeof(RequestAddLoaiNguyenLieu), typeof(LoaiNguyenLieu));
        CreateMap(typeof(RequestUpdateLoaiNguyenLieu), typeof(LoaiNguyenLieu));

        //Đơn vị tính
        CreateMap(typeof(DonViTinh), typeof(DonViTinhRespond));
        CreateMap(typeof(RequestAddDonViTinh), typeof(DonViTinh));
        CreateMap(typeof(RequestUpdateDonViTinh), typeof(DonViTinh));

        //Nguyên liệu
        // CreateMap(typeof(NguyenLieu), typeof(NguyenLieuRespond));
        CreateMap(typeof(RequestAddNguyenLieu), typeof(NguyenLieu));
        CreateMap(typeof(RequestUpdateNguyenLieu), typeof(NguyenLieu));

        //Danh mục món ăn
        CreateMap(typeof(DanhMucMonAn), typeof(DanhMucMonAnRespond));
        CreateMap(typeof(RequestAddDanhMucMonAn), typeof(DanhMucMonAn));
        CreateMap(typeof(RequestUpdateDanhMucMonAn), typeof(DanhMucMonAn));

        //Loại món ăn
        CreateMap<LoaiMonAn, LoaiMonAnRespond>()
            .ForMember(dest => dest.danhMucMonAn, opt => opt.Ignore());
        CreateMap(typeof(RequestAddLoaiMonAn), typeof(LoaiMonAn));
        CreateMap(typeof(RequestUpdateLoaiMonAn), typeof(LoaiMonAn));

        //Công thức
        // CreateMap(typeof(CongThuc), typeof(CongThucRespond));
        CreateMap(typeof(RequestAddCongThuc), typeof(CongThuc));
        CreateMap(typeof(RequestUpdateCongThuc), typeof(CongThuc));

        //NguyenLieuCongThuc
        CreateMap(typeof(NguyenLieuCongThuc), typeof(NguyenLieuCongThucRespond));
        CreateMap(typeof(LoaiNguyenLieuCongThuc), typeof(LoaiNguyenLieuCongThucRespond));


        //Khuyến mãi
        CreateMap(typeof(KhuyenMai), typeof(KhuyenMaiRespond));
        CreateMap(typeof(RequestAddKhuyenMai), typeof(KhuyenMai));
        CreateMap(typeof(RequestUpdateKhuyenMai), typeof(KhuyenMai));

        //Loại khuyến mãi
        CreateMap(typeof(LoaiKhuyenMai), typeof(LoaiKhuyenMaiRespond));
        CreateMap(typeof(RequestAddLoaiKhuyenMai), typeof(LoaiKhuyenMai));
        CreateMap(typeof(RequestUpdateLoaiKhuyenMai), typeof(LoaiKhuyenMai));

        //Món ăn
        // CreateMap(typeof(MonAn), typeof(MonAnRespond));
        CreateMap(typeof(RequestAddMonAn), typeof(MonAn));
        CreateMap(typeof(RequestUpdateMonAn), typeof(MonAn));

        //Ban
        CreateMap<Ban, BanRespond>()
            .ForMember(dest => dest.loaiBan, opt => opt.Ignore());
        CreateMap(typeof(RequestAddBan), typeof(Ban));
        CreateMap(typeof(RequestUpdateBan), typeof(Ban));

        //Combo
        // CreateMap(typeof(Combo), typeof(ComboRespond));
        CreateMap(typeof(RequestAddCombo), typeof(Combo));
        CreateMap(typeof(RequestUpdateCombo), typeof(Combo));

        //Loại bàn
        CreateMap(typeof(LoaiBan), typeof(LoaiBanRespond));
        CreateMap(typeof(RequestAddLoaiBan), typeof(LoaiBan));
        CreateMap(typeof(RequestUpdateLoaiBan), typeof(LoaiBan));

        //Thực đơn
        CreateMap(typeof(ThucDon), typeof(ThucDonRespond));
        CreateMap(typeof(RequestAddThucDon), typeof(ThucDon));
        CreateMap(typeof(RequestUpdateThucDon), typeof(ThucDon));

        //Loại tủ đồ
        CreateMap(typeof(LoaiTuDo), typeof(LoaiTuDoRespond));
        CreateMap(typeof(RequestAddLoaiTuDo), typeof(LoaiTuDo));
        CreateMap(typeof(RequestUpdateLoaiTuDo), typeof(LoaiTuDo));

        //Tủ đồ
        CreateMap(typeof(TuDo), typeof(TuDoRespond));
        CreateMap(typeof(RequestAddTuDo), typeof(TuDo));
        CreateMap(typeof(RequestUpdateTuDo), typeof(TuDo));

        //Khách Hàng
        CreateMap(typeof(KhachHang), typeof(KhachHangRespond));
        CreateMap(typeof(RequestAddKhachHang), typeof(KhachHang));
        CreateMap(typeof(RequestUpdateKhachHang), typeof(KhachHang));

        //Menu Dynamic
        CreateMap(typeof(MenuDynamic), typeof(MenuDynamicRespond));
        CreateMap(typeof(RequestAddMenuDynamic), typeof(MenuDynamic));
        CreateMap(typeof(RequestUpdateMenuDynamic), typeof(MenuDynamic));
        // CreateMap(typeof(List<MenuDynamic>), typeof(List<MenuDynamicRespond>));

        //Giảm giá
        CreateMap(typeof(GiamGia), typeof(GiamGiaRespond));
        CreateMap(typeof(RequestAddGiamGia), typeof(GiamGia));
        CreateMap(typeof(RequestUpdateGiamGia), typeof(GiamGia));

        //Đơn đặt bàn
        CreateMap(typeof(DonDatBan), typeof(DonDatBanRespond));
        CreateMap(typeof(RequestAddDonDatBan), typeof(DonDatBan));
        CreateMap(typeof(RequestUpdateDonDatBan), typeof(DonDatBan));

        //Đơn Order
        // CreateMap(typeof(DonOrder), typeof(DonOrderRespond));
        CreateMap<DonOrder, DonOrderRespond>()
            .ForMember(dest => dest.ban, opt => opt.Ignore())
           .ForMember(dest => dest.loaiDon, opt => opt.Ignore());
        CreateMap(typeof(RequestAddDonOrder), typeof(DonOrder));
        CreateMap(typeof(RequestUpdateDonOrder), typeof(DonOrder));

        //Loại đơn order
        CreateMap(typeof(LoaiDon), typeof(LoaiDonRespond));
        CreateMap(typeof(RequestAddLoaiDon), typeof(LoaiDon));
        CreateMap(typeof(RequestUpdateLoaiDon), typeof(LoaiDon));
    }
}
