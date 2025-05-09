using repo_nha_hang_com_ga_BE.Models.Common;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class MenuDynamic : BaseMongoDb
{
        public string? routeLink { get; set; }
        public string? icon { get; set; }
        public string? label { get; set; }
        public bool? isOpen { get; set; } = false;
        public string? parent { get; set; }
        public string? position { get; set; }
        public bool? isActive { get; set; } = true;
}

// public class MenuDynamicChild
// {
//         public string? routeLink { get; set; }
//         public string? icon { get; set; }
//         public string? label { get; set; }
//         public bool? isOpen { get; set; } = false;
//         public List<MenuDynamicChild>? children { get; set; }
// }
