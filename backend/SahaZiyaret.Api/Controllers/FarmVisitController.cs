/* Ziyaret endpoint’lerini (api/farm-visits) karşılar, asıl işi FarmVisitService’e yollaer. */

using Microsoft.AspNetCore.Mvc;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Services;

namespace SahaZiyaret.Api.Controllers;

[ApiController]
[Route("api/farm-visits")]
public class FarmVisitsController : ControllerBase
{
    private readonly FarmVisitService _service;

    public FarmVisitsController(FarmVisitService service)
    {
        _service = service;
    }

    /* GET api/farm-visits/{id}: Ziyareti getirir, bulunamazsa 404 döner. */
    [HttpGet("{id}")]
    public async Task<ActionResult<FarmVisitDto>> GetById(int id)
    {
        var visit = await _service.GetByIdAsync(id);
        if (visit == null)
        {
            return NotFound();
        }
        return visit;
    }

    /* POST api/farm-visits: Yeni ziyaret oluşturur. Çiftçi yoksa 400, başarılıysa 201 Created ve oluşan kaydı döner. */
    [HttpPost]
    public async Task<ActionResult<FarmVisitDto>> Create(FarmVisitRequest request)
    {
        var created = await _service.CreateAsync(request);
        if (created == null)
        {
            return BadRequest("Belirtilen çiftçi bulunamadı.");
        }
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}