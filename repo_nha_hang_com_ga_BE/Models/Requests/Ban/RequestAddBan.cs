using repo_nha_hang_com_ga_BE.Models.Common.Models;
namespace repo_nha_hang_com_ga_BE.Models.Requests.Ban;

public class RequestAddBan
{
    public string? tenBan { get; set; }

    public string? loaiBan { get; set; }

    public TrangThaiBan? trangThai { get; set; }
}