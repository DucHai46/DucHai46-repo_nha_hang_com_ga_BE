using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;

namespace repo_nha_hang_com_ga_BE.Models.Requests.LoaiDon;

public class RequestSearchLoaiDon : PagingParameterModel
{
    public string? tenLoaiDon { get; set; }
}