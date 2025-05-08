using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Responds.Combo;

public class ComboRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    public string? tenCombo { get; set; }
    public List<LoaiMonAnMenuRespond>? loaiMonAns { get; set; }
    public string? hinhAnh { get; set; }
    public int? giaTien { get; set; }
    public string? moTa { get; set; }
}

public class LoaiMonAnMenuRespond : IdName
{
    public List<MonAnMenuRespond>? monAns { get; set; }
    public string? moTa { get; set; }

}

public class MonAnMenuRespond
{
    public string? id { get; set; }
    public string? tenMonAn { get; set; }
    public string? hinhAnh { get; set; }
    public int? giaTien { get; set; }
    public string? moTa { get; set; }
}