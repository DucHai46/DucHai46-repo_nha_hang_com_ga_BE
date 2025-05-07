using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.NguyenLieu;

public class RequestUpdateNguyenLieu
{
    public string? tenNguyenLieu { get; set; }

    public string? moTa { get; set; }

    // public DateTimeOffset? hanSuDung { get; set; }
    public int? soLuong { get; set; }

    public string? loaiNguyenLieu { get; set; }

    public string? donViTinh { get; set; }

    public string? tuDo { get; set; }

    public TrangThaiNguyenLieu? trangThai { get; set; }
}