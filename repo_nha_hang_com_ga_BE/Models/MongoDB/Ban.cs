using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;
using System.ComponentModel;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB
{
    public class Ban : BaseMongoDb
    {
        public string? tenBan { get; set; }
        public string? loaiBan { get; set; }
        public TrangThaiBan? trangThai { get; set; }
    }
}

public enum TrangThaiBan
{
    [Description("Trống")]
    Trong = 0,
    [Description("Có khách")]
    CoKhach = 1,
    [Description("Đã đặt")]
    DaDat = 2,
}
