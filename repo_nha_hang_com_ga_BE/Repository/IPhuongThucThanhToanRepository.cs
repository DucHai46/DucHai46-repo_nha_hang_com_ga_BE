using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests;
using repo_nha_hang_com_ga_BE.Models.Responds;
using repo_nha_hang_com_ga_BE.Models.Responds.PhuongThucThanhToan;



namespace repo_nha_hang_com_ga_BE.Repository;

public interface IPhuongThucThanhToanRepository
{
    Task<RespondAPIPaging<List<PhuongThucThanhToanRespond>>> GetAllPhuongThucThanhToans(RequestSearchPhuongThucThanhToan request);
    Task<RespondAPI<PhuongThucThanhToanRespond>> GetPhuongThucThanhToanById(string id);
    Task<RespondAPI<PhuongThucThanhToanRespond>> CreatePhuongThucThanhToan(RequestAddPhuongThucThanhToan product);
    Task<RespondAPI<PhuongThucThanhToanRespond>> UpdatePhuongThucThanhToan(string id, RequestUpdatePhuongThucThanhToan product);
    Task<RespondAPI<string>> DeletePhuongThucThanhToan(string id);
}