using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class TuDo : BaseMongoDb
{
    public string? tenTuDo { get; set; }
    public string? moTa { get; set; }
    public string? loaiTuDo { get; set; }
}

