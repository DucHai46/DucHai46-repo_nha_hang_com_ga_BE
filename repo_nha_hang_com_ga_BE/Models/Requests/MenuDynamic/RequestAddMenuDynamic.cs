using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.MenuDynamic;

public class RequestAddMenuDynamic
{
    public string? routeLink { get; set; }
    public string? icon { get; set; }
    public string? label { get; set; }
    public bool? isOpen { get; set; } = false;
    public string? parent { get; set; }
    public string? position { get; set; }
    public bool? isActive { get; set; } = true;
}

