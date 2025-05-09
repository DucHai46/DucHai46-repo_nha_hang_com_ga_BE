using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.MongoDB;
using repo_nha_hang_com_ga_BE.Models.Requests.NhanVien;
using repo_nha_hang_com_ga_BE.Models.Responds.NhanVien;

namespace repo_nha_hang_com_ga_BE.Repository;

public interface INhanVienRepository
{
    Task<RespondAPIPaging<List<NhanVienRespond>>> GetAllNhanViens(RequestSearchNhanVien request);
    Task<RespondAPI<NhanVienRespond>> GetNhanVienById(string id);
    Task<RespondAPI<NhanVienRespond>> CreateNhanVien(RequestAddNhanVien product);
    Task<RespondAPI<NhanVienRespond>> UpdateNhanVien(string id, RequestUpdateNhanVien product);
    Task<RespondAPI<string>> DeleteNhanVien(string id);
}

