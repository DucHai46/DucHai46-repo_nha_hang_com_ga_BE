﻿namespace repo_nha_hang_com_ga_BE.Models.Requests.Combo;

public class RequestUpdateCombo
{
    public string? tenCombo { get; set; }
    public List<MonAnMenu>? monAns { get; set; }
    public string? hinhAnh { get; set; }
    public string? giaTien { get; set; }
    public string? moTa { get; set; }
}