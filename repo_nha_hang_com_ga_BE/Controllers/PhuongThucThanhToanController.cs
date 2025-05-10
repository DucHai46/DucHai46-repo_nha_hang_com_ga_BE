using Microsoft.AspNetCore.Mvc;
// using repo_nha_hang_com_ga_BE.Models.Requests.PhuongThucThanhToan;
using repo_nha_hang_com_ga_BE.Repository;
using Microsoft.AspNetCore.Authorization;
using repo_nha_hang_com_ga_BE.Models.Requests;

namespace repo_nha_hang_com_ga_BE.Controllers;


[Authorize]
[ApiController]
[Route("api/phuong-thuc-thanh-toan")]


public class PhuongThucThanhToanController : ControllerBase
{
    private readonly IPhuongThucThanhToanRepository _PhuongThucThanhToanRepository;

    public PhuongThucThanhToanController(IPhuongThucThanhToanRepository PhuongThucThanhToanRepository)
    {
        _PhuongThucThanhToanRepository = PhuongThucThanhToanRepository;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllPhuongThucThanhToans([FromQuery] RequestSearchPhuongThucThanhToan request)
    {
        return Ok(await _PhuongThucThanhToanRepository.GetAllPhuongThucThanhToans(request));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhuongThucThanhToanById(string id)
    {
        return Ok(await _PhuongThucThanhToanRepository.GetPhuongThucThanhToanById(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreatePhuongThucThanhToan(RequestAddPhuongThucThanhToan request)
    {
        return Ok(await _PhuongThucThanhToanRepository.CreatePhuongThucThanhToan(request));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePhuongThucThanhToan(string id, RequestUpdatePhuongThucThanhToan request)
    {
        return Ok(await _PhuongThucThanhToanRepository.UpdatePhuongThucThanhToan(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhuongThucThanhToan(string id)
    {
        return Ok(await _PhuongThucThanhToanRepository.DeletePhuongThucThanhToan(id));
    }
}
