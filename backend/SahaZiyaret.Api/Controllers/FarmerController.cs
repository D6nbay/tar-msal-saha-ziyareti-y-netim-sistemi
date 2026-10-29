/* Çiftçi endpoint’lerini (api/Farmers) karşılar, asıl işi FarmerService’e devreder. */

using Microsoft.AspNetCore.Mvc;
using SahaZiyaret.Api.Dtos;
using SahaZiyaret.Api.Services;

namespace SahaZiyaret.Api.Controllers;

[ApiController]
[Route("api/[controller]")]//controller atılmış hali gelmeli
public class FarmersController : ControllerBase//ControllerBase, HTTP cevabı üretmek için gereken hazır araçları getrir
{
    private readonly FarmerService _service;
    //readonly sınıf dışından görülmez ve sadece constructorda bir kez atanır demek unutma

    public FarmersController(FarmerService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    //get isteği uyuşunca çalışır
    /* GET api/Farmers/{id}: Çiftçiyi getirir, bulunamazsa 404*/
    public async Task<ActionResult<FarmerDto>> GetById(int id){
        //durum kodu veya farmerDto döndürebilmeli
        var farmer = await _service.GetByIdAsync(id);
        if(farmer == null){
            return NotFound();
        }
        return farmer;
    }

    /*Çiftçileri search, page ve pageSize parametreleriyle sayfalı olarak listeler. */
    [HttpGet]
    public async Task<ActionResult<PagedResult<FarmerDto>>> GetAll(string? search, int page = 1, int pageSize = 10)
    {
        var result = await _service.GetAllAsync(search, page, pageSize);
        return result;
    }

    [HttpPost]
    //post isteği adrese glince çalışır
    /* POST api/Farmers: Yeni çiftçi oluşturur, 201 Created ve oluşan kaydı döner. */
    public async Task<ActionResult<FarmerDto>> Create(FarmerRequest request){
        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id}, created);
    }
}