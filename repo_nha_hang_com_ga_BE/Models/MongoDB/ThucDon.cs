using System.ComponentModel;
using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class ThucDon : BaseMongoDb
{
    public string? tenThucDon { get; set; }
    public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public List<ComboMenu>? combos { get; set; }
    public TrangThaiThucDon? trangThai { get; set; }
}

public class ComboMenu
{
    public string? id { get; set; }
}

public enum TrangThaiThucDon
{
    [Description("Chưa hoạt động")]
    ChuaHoatDong = 0,
    [Description("Đã hoạt động")]
    DaHoatDong = 1,
}



