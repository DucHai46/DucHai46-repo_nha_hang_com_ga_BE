using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using repo_nha_hang_com_ga_BE.Models.Common.Respond;
using repo_nha_hang_com_ga_BE.Models.Requests.ChucVu;
using repo_nha_hang_com_ga_BE.Models.Responds.ChucVu;
using repo_nha_hang_com_ga_BE.Repository;

namespace repo_nha_hang_com_ga_BE.Controllers;

[Authorize]
[ApiController]
[Route("api/chuc-vu")]
public class ChucVuController : ControllerBase
{
    private readonly IChucVuRepository _repository;

    public ChucVuController(IChucVuRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("")] // định nghĩa route cho phương thức này
    public async Task<IActionResult> GetAllChucVus([FromQuery] RequestSearchChucVu request) // 
    {
        return Ok(await _repository.GetAllChucVus(request));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetChucVuById(string id)
    {
        return Ok(await _repository.GetChucVuById(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateChucVu(RequestAddChucVu request)
    {
        return Ok(await _repository.CreateChucVu(request));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateChucVu(string id, RequestUpdateChucVu request)
    {
        return Ok(await _repository.UpdateChucVu(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChucVu(string id)
    {
        return Ok(await _repository.DeleteChucVu(id));
    }
}