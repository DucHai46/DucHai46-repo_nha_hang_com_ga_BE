using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.MenuDynamic;

public class RequestUpdateMenuDynamic
{
    public string? routeLink { get; set; }
    public string? icon { get; set; }
    public string? label { get; set; }
    public bool? isOpen { get; set; } = false;
    public List<MenuDynamicChild>? children { get; set; }

}