using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB
{
    public class Combo : BaseMongoDb
    {
        public string? tenCombo { get; set; }
        public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
        public string? hinhAnh { get; set; }
        public string? giaTien { get; set; }
        public string? moTa { get; set; }
    }
}

public class LoaiMonAnMenu
{
    public string? id { get; set; }
    public List<MonAnMenu>? monAns { get; set; }
    public string? moTa { get; set; }

}

public class MonAnMenu
{
    public string? id { get; set; }
    public string? tenMonAn { get; set; }
    public string? hinhAnh { get; set; }
    public string? giaTien { get; set; }
    public string? moTa { get; set; }
}


