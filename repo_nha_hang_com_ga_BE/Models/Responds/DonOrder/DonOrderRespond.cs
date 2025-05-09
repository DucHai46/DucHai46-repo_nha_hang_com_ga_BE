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
    public IdName? khachHang { get; set; }
    public TrangThaiDonOrder? trangThai { get; set; }
    public List<ChiTietDonOrderRespond>? chiTietDonOrder { get; set; }
    public int? tongTien { get; set; }

}

public class ChiTietDonOrderRespond
{

    public List<DonMonAnRespond>? monAns { get; set; }

    public int? trangThai { get; set; }

}

public class DonMonAnRespond
{

    public IdName? monAn { get; set; }

    public TrangThaiDonMonAn? monAn_trangThai { get; set; }

    public int? soLuong { get; set; }

    public int? giaTien { get; set; }

    public string? moTa { get; set; }

}