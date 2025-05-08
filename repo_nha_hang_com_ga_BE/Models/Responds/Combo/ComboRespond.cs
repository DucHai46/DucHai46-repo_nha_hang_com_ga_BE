using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.Responds.Combo;

public class ComboRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    public string? tenCombo { get; set; }
    public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public string? hinhAnh { get; set; }
    public int? giaTien { get; set; }
    public string? moTa { get; set; }
}