
using System.ComponentModel;
using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;
public class HoaDonThanhToan : BaseMongoDb
{
    public string? tenHoaDon { get; set; }
    public string? qrCode { get; set; }
    public DateTime? gioVao { get; set; }
    public DateTime? gioRa { get; set; }
    public int? soNguoi { get; set; }
    public TrangThaiHoaDon? trangthai { get; set; }

}

public enum TrangThaiHoaDon
{
    [Description("Chưa thanh toán")]
    ChuaThanhToan = 0,
    [Description("Đã thanh toán")]
    DaThanhToan = 1,
}