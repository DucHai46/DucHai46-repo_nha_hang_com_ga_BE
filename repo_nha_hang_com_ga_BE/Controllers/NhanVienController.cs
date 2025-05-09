using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests.NhanVien;
using repo_nha_hang_com_ga_BE.Models.Responds.NhanVien;
using repo_nha_hang_com_ga_BE.Repository;

namespace repo_nha_hang_com_ga_BE.Controllers;

[Authorize]
[ApiController]
[Route("api/nhan-vien")]
public class NhanVienController : ControllerBase
{
    private readonly INhanVienRepository _repository;

    public NhanVienController(INhanVienRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("")] // định nghĩa route cho phương thức này
    public async Task<IActionResult> GetAllNhanViens([FromQuery] RequestSearchNhanVien request) // 
    {
        return Ok(await _repository.GetAllNhanViens(request));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNhanVienById(string id)
    {
        return Ok(await _repository.GetNhanVienById(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateNhanVien(RequestAddNhanVien request)
    {
        return Ok(await _repository.CreateNhanVien(request));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNhanVien(string id, RequestUpdateNhanVien request)
    {
        return Ok(await _repository.UpdateNhanVien(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNhanVien(string id)
    {
        return Ok(await _repository.DeleteNhanVien(id));
    }
}