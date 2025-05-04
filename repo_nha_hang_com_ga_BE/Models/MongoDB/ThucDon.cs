using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class ThucDon : BaseMongoDb
{
    public string? tenThucDon { get; set; }
    public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public List<ComboMenu>? combos { get; set; }
}

public class ComboMenu : IdName
{
    // public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public string? hinhAnh { get; set; }
    public string? giaTien { get; set; }
    public string? moTa { get; set; }
}


