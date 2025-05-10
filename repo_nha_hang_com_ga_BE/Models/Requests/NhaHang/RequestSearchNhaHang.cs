using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;

namespace repo_nha_hang_com_ga_BE.Models.Requests.NhaHang;

public class RequestSearchNhaHang : PagingParameterModel
{
    public string? tenNhaHang { get; set; }

    public bool? isActive { get; set; }
}
