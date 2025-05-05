using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;

public class RequestUpdateThucDon
{
    public string? tenThucDon { get; set; }
    public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public List<ComboMenu>? combos { get; set; }
    public TrangThaiThucDon? trangThai { get; set; }
}