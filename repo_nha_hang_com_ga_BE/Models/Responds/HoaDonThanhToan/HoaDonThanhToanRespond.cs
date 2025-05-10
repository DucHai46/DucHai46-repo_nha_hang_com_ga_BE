using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Responds.HoaDonThanhToan;

public class HoaDonThanhToanRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    public IdName? nhanVien { get; set; }
    public IdName? donOrder { get; set; }
    public IdName? phuongThucThanhToan { get; set; }
    public IdName? nhaHang { get; set; }
    public string? tenHoaDon { get; set; }
    public string? qrCode { get; set; }
    public DateTimeOffset? gioVao { get; set; }
    public DateTimeOffset? gioRa { get; set; }
    public int? soNguoi { get; set; }
    public IdName? khuyenMai { get; set; }
    public IdName? phuPhi { get; set; }
    public TrangThaiHoaDon? trangthai { get; set; }
}