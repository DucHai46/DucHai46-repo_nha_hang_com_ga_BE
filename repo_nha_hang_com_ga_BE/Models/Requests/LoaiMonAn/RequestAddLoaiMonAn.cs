using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Requests.LoaiMonAn;

public class RequestAddLoaiMonAn
{
    public string? tenLoai { get; set; }

    public string? moTa { get; set; }

    public string? danhMucMonAn { get; set; }
}