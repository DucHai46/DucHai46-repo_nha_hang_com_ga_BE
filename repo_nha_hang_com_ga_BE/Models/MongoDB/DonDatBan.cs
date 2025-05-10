using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB
{
    public class DonDatBan : BaseMongoDb
    {
        public string? ban { get; set; }

        public string? khachHang { get; set; }

        public string? khungGio { get; set; }

    }
}