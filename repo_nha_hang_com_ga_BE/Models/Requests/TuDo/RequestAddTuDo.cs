using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Requests.TuDo;

public class RequestAddTuDo
{
    public string? tenTuDo { get; set; }

    public string? moTa { get; set; }

    public string? loaiTuDo { get; set; }
}