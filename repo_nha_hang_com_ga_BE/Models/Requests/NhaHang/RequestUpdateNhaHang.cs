using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Requests.NhaHang;

public class RequestUpdateNhaHang
{
    public string? tenNhaHang { get; set; }
    public string? diaChi { get; set; }
    public string? soDienThoai { get; set; }
    public string? email { get; set; }
    public string? website { get; set; }
    public string? logo { get; set; }
    public string? banner { get; set; }
    public string? moTa { get; set; }
    public bool isActive { get; set; }
}
