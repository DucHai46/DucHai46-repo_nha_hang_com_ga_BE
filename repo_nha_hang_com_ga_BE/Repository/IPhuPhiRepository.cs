using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests;
using repo_nha_hang_com_ga_BE.Models.Responds;
using repo_nha_hang_com_ga_BE.Models.Responds.PhuPhi;



namespace repo_nha_hang_com_ga_BE.Repository;

public interface IPhuPhiRepository
{
    Task<RespondAPIPaging<List<PhuPhiRespond>>> GetAllPhuPhis(RequestSearchPhuPhi request);
    Task<RespondAPI<PhuPhiRespond>> GetPhuPhiById(string id);
    Task<RespondAPI<PhuPhiRespond>> CreatePhuPhi(RequestAddPhuPhi product);
    Task<RespondAPI<PhuPhiRespond>> UpdatePhuPhi(string id, RequestUpdatePhuPhi product);
    Task<RespondAPI<string>> DeletePhuPhi(string id);
}