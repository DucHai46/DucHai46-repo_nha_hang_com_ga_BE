using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;
namespace repo_nha_hang_com_ga_BE.Models.Requests.MenuDynamic;

public class RequestSearchMenuDynamic : PagingParameterModel
{
    public string? label { get; set; }
}