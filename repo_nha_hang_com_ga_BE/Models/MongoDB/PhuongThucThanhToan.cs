using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class PhuongThucThanhToan : BaseMongoDb
{
    public string? tenPhuongThuc { get; set; }
    public string? moTa { get; set; }

}