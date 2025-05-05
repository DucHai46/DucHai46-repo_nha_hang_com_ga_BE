using repo_nha_hang_com_ga_BE.Models.Common.Models;
using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;
using repo_nha_hang_com_ga_BE.Models.MongoDB;

namespace repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;

public class RequestSearchThucDon : PagingParameterModel
{
    public string? tenThucDon { get; set; }
    public string? loaiMonAnId { get; set; }
    public string? comboId { get; set; }
    public TrangThaiThucDon? trangThai { get; set; }
}