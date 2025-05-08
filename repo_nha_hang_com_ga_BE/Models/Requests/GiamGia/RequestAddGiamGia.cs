using repo_nha_hang_com_ga_BE.Models.Common.Models;

namespace repo_nha_hang_com_ga_BE.Models.Requests.GiamGia;

public class RequestAddGiamGia
{
    public string? tenGiamGia { get; set; }
    public string? moTa { get; set; }
    public DateTimeOffset ngayBatDau { get; set; }
    public DateTimeOffset ngayKetThuc { get; set; }
    public string? giaTri { get; set; }
}