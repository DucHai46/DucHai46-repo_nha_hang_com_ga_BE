using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Responds.DonOrder;

public class DonOrderRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public string? tenDon { get; set; }
    public IdName? loaiDon { get; set; }
    public IdName? ban { get; set; }
    public TrangThaiDonOrder? trangThai { get; set; }
    public List<ChiTietDonOrder>? chiTietDonOrder { get; set; }
    public int? tongTien { get; set; }

}