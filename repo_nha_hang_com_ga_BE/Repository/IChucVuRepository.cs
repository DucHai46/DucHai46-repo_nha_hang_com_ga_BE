using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests.ChucVu;
using repo_nha_hang_com_ga_BE.Models.Responds.ChucVu;

namespace repo_nha_hang_com_ga_BE.Repository;

public interface IChucVuRepository
{
    Task<RespondAPIPaging<List<ChucVuRespond>>> GetAllChucVus(RequestSearchChucVu request);
    Task<RespondAPI<ChucVuRespond>> GetChucVuById(string id);
    Task<RespondAPI<ChucVuRespond>> CreateChucVu(RequestAddChucVu product);
    Task<RespondAPI<ChucVuRespond>> UpdateChucVu(string id, RequestUpdateChucVu product);
    Task<RespondAPI<string>> DeleteChucVu(string id);
}