using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiDon;
using repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;
using repo_nha_hang_com_ga_BE.Models.Responds.LoaiDon;

namespace repo_nha_hang_com_ga_BE.Models.Repositories;
public interface ILoaiDonRepository
{
    Task<RespondAPIPaging<List<LoaiDonRespond>>> GetAllLoaiDon(RequestSearchLoaiDon request);
    Task<RespondAPI<LoaiDonRespond>> GetLoaiDonById(string id);

    Task<RespondAPI<LoaiDonRespond>> CreateLoaiDon(RequestAddLoaiDon request);
    Task<RespondAPI<LoaiDonRespond>> UpdateLoaiDon(string id, RequestUpdateLoaiDon request);
    Task<RespondAPI<string>> DeleteLoaiDon(string id);


}