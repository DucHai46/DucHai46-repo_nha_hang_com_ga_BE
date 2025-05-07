using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class NguyenLieu : BaseMongoDb
{
    public string? tenNguyenLieu { get; set; }
    // public DateTimeOffset? hanSuDung { get; set; }
    public int? soLuong { get; set; }
    public string? moTa { get; set; }
    public string? loaiNguyenLieu { get; set; }
    public string? donViTinh { get; set; }

    public string? tuDo { get; set; }

    public TrangThaiNguyenLieu? trangThai { get; set; }

}

public enum TrangThaiNguyenLieu
{
    ChuaSuDung,
    DangSuDung,
    HetHan,
}
