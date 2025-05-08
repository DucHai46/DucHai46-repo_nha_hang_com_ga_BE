namespace repo_nha_hang_com_ga_BE.Models.Requests.Combo;

public class RequestUpdateCombo
{
    public string? tenCombo { get; set; }
    public List<LoaiMonAnMenu>? loaiMonAns { get; set; }
    public string? hinhAnh { get; set; }
    public int? giaTien { get; set; }
    public string? moTa { get; set; }
}