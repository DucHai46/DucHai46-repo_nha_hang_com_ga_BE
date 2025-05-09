using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;

namespace repo_nha_hang_com_ga_BE.Models.Requests.ChucVu;

public class RequestSearchChucVu : PagingParameterModel
{
    public string? tenChucVu { get; set; }

}