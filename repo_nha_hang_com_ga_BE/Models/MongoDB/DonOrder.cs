using System.ComponentModel;
using MongoDB.Driver;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Responds.MonAn;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class DonOrder : BaseMongoDb
{
    public string? tenDon { get; set; }

    public string? loaiDon { get; set; }

    public string? ban { get; set; }
    public string? khachHang { get; set; }

    public TrangThaiDonOrder? trangThai { get; set; }

    public List<ChiTietDonOrder>? chiTietDonOrder { get; set; }
    public int? tongTien { get; set; }

}


public enum TrangThaiDonOrder
{
    [Description("Chưa thanh toán")]
    ChuaThanhToan = 0,
    [Description("Đã thanh toán")]
    DaThanhToan = 1,

}

public class ChiTietDonOrder
{
    // public string? id { get; set; }
    public List<DonMonAn>? monAns { get; set; }
    public List<DonComBo>? comBos { get; set; }

    public int? trangThai { get; set; }
}

public class DonMonAn
{

    public string? monAn { get; set; }

    public TrangThaiDonMonAn? monAn_trangThai { get; set; }

    public int? soLuong { get; set; }

    // public int? giaTien { get; set; }

    public string? moTa { get; set; }
}

public class DonComBo
{

    public string? comBo { get; set; }

    public TrangThaiDonMonAn? comBo_trangThai { get; set; }

    public int? soLuong { get; set; }
    public string? moTa { get; set; }
}

public enum TrangThaiDonMonAn
{
    [Description("Đang chế biến")]
    DangCheBien = 0,
    [Description("Đã phục vụ")]
    DaPhucVu = 1,
}