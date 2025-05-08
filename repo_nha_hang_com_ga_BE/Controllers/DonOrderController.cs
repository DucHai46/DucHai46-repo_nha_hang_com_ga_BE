using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using repo_nha_hang_com_ga_BE.Models.Requests.DonOrder;
using repo_nha_hang_com_ga_BE.Repository;

namespace repo_nha_hang_com_ga_BE.Controllers;

[Authorize]
[ApiController]
[Route("api/don-order")]

public class DonOrderController : ControllerBase
{
    private readonly IDonOrderRepository _repository;

    public DonOrderController(IDonOrderRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetAllDonOrder([FromQuery] RequestSearchDonOrder request)
    {
        return Ok(await _repository.GetAllDonOrder(request));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDonOrderById(string id)
    {
        return Ok(await _repository.GetDonOrderById(id));
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateDonOrder(RequestAddDonOrder request)
    {
        return Ok(await _repository.CreateDonOrder(request));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDonOrder(string id, RequestUpdateDonOrder request)
    {
        return Ok(await _repository.UpdateDonOrder(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDonOrder(string id)
    {
        return Ok(await _repository.DeleteDonOrder(id));
    }
}
