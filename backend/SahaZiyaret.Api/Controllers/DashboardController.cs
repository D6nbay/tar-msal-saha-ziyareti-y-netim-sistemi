using Microsoft.AspNetCore.Mvc;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Services;

namespace SahaZiyaret.Api.Controllers;

/* Dashboard endpoint'ini (api/dashboard) karşılar, işi DashboardService'e devreder. */
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    /* GET api/dashboard/summary: Dashboard özet bilgilerini döndürür. */
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var summary = await _service.GetSummaryAsync();
        return summary;
    }
}