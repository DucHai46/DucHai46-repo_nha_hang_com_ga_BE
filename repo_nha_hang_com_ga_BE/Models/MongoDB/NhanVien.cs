using repo_nha_hang_com_ga_BE.Models.Common;
using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.MongoDB;

public class NhanVien : BaseMongoDb
{
    public string? tenNhanVien { get; set; }
    public string? soDienThoai { get; set; }
    public string? email { get; set; }
    public string? diaChi { get; set; }
    public string? ngaySinh { get; set; }
    public string? chucVu { get; set; }
}

