
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace repo_nha_hang_com_ga_BE.Models.Responds.LoaiDon;
public class LoaiDonRespond
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? id { get; set; }
    public string? tenLoaiDon { get; set; }
    public string? moTa { get; set; }

}