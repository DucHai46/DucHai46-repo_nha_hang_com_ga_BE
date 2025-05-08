using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Responds.Common;

namespace repo_nha_hang_com_ga_BE.Models.Responds.MonAn;

public class MonAnRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }

    public string? tenMonAn { get; set; }

    public IdName? loaiMonAn { get; set; }

    public IdName? congThuc { get; set; }

    public GiamGiaMonAnRespond? giamGia { get; set; }

    public string? hinhAnh { get; set; }

    public string? giaTien { get; set; }

    public string? moTa { get; set; }
}

public class GiamGiaMonAnRespond : IdName
{
    public string? giaTri { get; set; }
}