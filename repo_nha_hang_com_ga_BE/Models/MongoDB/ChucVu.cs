using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class ChucVu : BaseMongoDb
{
    public string? tenChucVu { get; set; }
    public string? moTa { get; set; }
}