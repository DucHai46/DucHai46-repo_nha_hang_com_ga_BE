using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;

namespace repo_nha_hang_com_ga_BE.Models.Requests.Combo;

public class RequestSearchCombo : PagingParameterModel
{
    public string? tenCombo { get; set; }
    public string? giaTien { get; set; }
}