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

    public IdName? ban { get; set; }

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
    public List<DoMonAn>? monAns { get; set; }

    public int? _trangThai { get; set; }
}

public class DoMonAn
{
    public IdName? monAn { get; set; }

    public TrangThaiDonMonAn? monAn_trangThai { get; set; }

    public int? soLuong { get; set; }

    public int? giaTien { get; set; }

    public string? moTa { get; set; }
}

public enum TrangThaiDonMonAn
{
    [Description("Chưa chế biến")]
    DangCheBien = 0,
    [Description("Đã phục vụ")]
    DaPhucVu = 1,
}