using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Responds.ThucDon;

public class ThucDonRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public string? tenThucDon { get; set; }
    public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public List<ComboMenu>? combos { get; set; }
    public TrangThaiThucDon? trangThai { get; set; }
}