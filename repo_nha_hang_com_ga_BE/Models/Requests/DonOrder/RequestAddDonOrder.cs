using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.DonOrder;
public class RequestAddDonOrder
{
    public string? tenDon { get; set; }
    public string? loaiDon { get; set; }
    public string? ban { get; set; }
    public TrangThaiDonOrder? trangThai { get; set; }
    public List<ChiTietDonOrder>? chiTietDonOrder { get; set; }
    public int? tongTien { get; set; }

}