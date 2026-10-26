using Microsoft.AspNetCore.Mvc;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Services;

namespace SahaZiyaret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FarmersController : ControllerBase
{
    private readonly FarmerService _service;

    public FarmersController(FarmerService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FarmerDto>> GetById(int id){
        var farmer = await _service.GetByIdAsync(id);
        if(farmer == null){
            return NotFound();
        }
        return farmer;
    }

    [HttpPost]
    public async Task<ActionResult<FarmerDto>> Create(FarmerRequest request){
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id}, created);
    }
}