using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using repo_nha_hang_com_ga_BE.Models.Repositories;
using repo_nha_hang_com_ga_BE.Models.Requests.LoaiDon;
using repo_nha_hang_com_ga_BE.Models.Requests.ThucDon;
using repo_nha_hang_com_ga_BE.Repository;

namespace repo_nha_hang_com_ga_BE.Controllers;

[Authorize]
[ApiController]
[Route("api/loai-don")]

public class LoaiDonController : ControllerBase
{
    private readonly ILoaiDonRepository _repository;

    public LoaiDonController(ILoaiDonRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllLoaiDon([FromQuery] RequestSearchLoaiDon request)
    {
        return Ok(await _repository.GetAllLoaiDon(request));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLoaiDonById(string id)
    {
        return Ok(await _repository.GetLoaiDonById(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateLoaiDon(RequestAddLoaiDon request)
    {
        return Ok(await _repository.CreateLoaiDon(request));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLoaiDon(string id, RequestUpdateLoaiDon request)
    {
        return Ok(await _repository.UpdateLoaiDon(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLoaiDon(string id)
    {
        return Ok(await _repository.DeleteLoaiDon(id));
    }
}
