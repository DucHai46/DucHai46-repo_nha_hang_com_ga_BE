using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.HoaDonThanhToan;

public class RequestUpdateHoaDonThanhToan
{
    public string? nhanVien { get; set; }
    public string? donOrder { get; set; }
    public string? phuongThucThanhToan { get; set; }
    public string? nhaHang { get; set; }
    public string? tenHoaDon { get; set; }
    public string? qrCode { get; set; }
    public DateTimeOffset? gioVao { get; set; }
    public DateTimeOffset? gioRa { get; set; }
    public int? soNguoi { get; set; }
    public string? khuyenMai { get; set; }
    public string? phuPhi { get; set; }
    public TrangThaiHoaDon? trangthai { get; set; }
}