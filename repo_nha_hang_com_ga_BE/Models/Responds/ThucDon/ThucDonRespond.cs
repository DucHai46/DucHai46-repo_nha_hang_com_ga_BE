using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Responds.Combo;

namespace repo_nha_hang_com_ga_BE.Models.Responds.ThucDon;

public class ThucDonRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public string? tenThucDon { get; set; }
    public List<LoaiMonAnMenuRespond>? loaiMonAns { get; set; }
    public List<ComboMenuRespond>? combos { get; set; }
    public TrangThaiThucDon? trangThai { get; set; }
}

public class ComboMenuRespond : IdName
{
    public string? hinhAnh { get; set; }
    public int? giaTien { get; set; }
    public string? moTa { get; set; }
}


