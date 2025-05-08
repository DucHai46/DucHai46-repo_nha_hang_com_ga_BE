using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;

namespace repo_nha_hang_com_ga_BE.Models.Requests;

public class RequestSearchPhuongThucThanhToan : PagingParameterModel
{
    public string? tenPhuongThuc { get; set; }

}