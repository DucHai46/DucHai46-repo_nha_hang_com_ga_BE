using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Responds.NhanVien;

public class NhanVienRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    public string? tenNhanVien { get; set; }

    public string? soDienThoai { get; set; }

    public string? email { get; set; }

    public string? diaChi { get; set; }

    public string? ngaySinh { get; set; }

    public IdName? chucVu { get; set; }
}