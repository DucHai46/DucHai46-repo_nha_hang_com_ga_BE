using repo_nha_hang_com_ga_BE.Models.Common.Models.Respond;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests.HoaDonThanhToan;
using repo_nha_hang_com_ga_BE.Models.Responds.HoaDonThanhToan;


namespace repo_nha_hang_com_ga_BE.Repository;

public interface IHoaDonThanhToanRepository
{
    Task<RespondAPIPaging<List<HoaDonThanhToanRespond>>> GetAllHoaDonThanhToan(RequestSearchHoaDonThanhToan request);
    Task<RespondAPI<HoaDonThanhToanRespond>> GetHoaDonThanhToanById(string id);
    Task<RespondAPI<HoaDonThanhToanRespond>> CreateHoaDonThanhToan(RequestAddHoaDonThanhToan request);
    Task<RespondAPI<HoaDonThanhToanRespond>> UpdateHoaDonThanhToan(string id, RequestUpdateHoaDonThanhToan request);
    Task<RespondAPI<string>> DeleteHoaDonThanhToan(string id);

}