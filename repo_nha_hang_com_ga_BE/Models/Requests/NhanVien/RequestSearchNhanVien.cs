using repo_nha_hang_com_ga_BE.Models.Common.Models.Request;

namespace repo_nha_hang_com_ga_BE.Models.Requests.NhanVien;

public class RequestSearchNhanVien : PagingParameterModel
{
    public string? tenNhanVien { get; set; }

    public string? chucVuId { get; set; }
}