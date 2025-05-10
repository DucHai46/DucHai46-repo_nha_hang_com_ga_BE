using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;
public class PhuPhi : BaseMongoDb
{

    public string? tenPhuPhi { get; set; }

    public int? giaTri { get; set; }

    public string? moTa { get; set; }

    public int? trangThai { get; set; }
}