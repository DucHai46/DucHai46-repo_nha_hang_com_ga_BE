using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Requests.DonDatBan;
public class RequestAddDonDatBan
{
    public string? ban { get; set; }
    public string? khachHang { get; set; }
    public string? khungGio { get; set; }

}