using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class NhaHang : BaseMongoDb
{
    public string? tenNhaHang { get; set; }
    public string? diaChi { get; set; }
    public string? soDienThoai { get; set; }
    public string? email { get; set; }
    public string? website { get; set; }
    public string? logo { get; set; }
    public string? banner { get; set; }
    public string? moTa { get; set; }
}