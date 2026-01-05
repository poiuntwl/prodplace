using Microsoft.AspNetCore.Mvc;

namespace Prodplace.Admin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CurrenciesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("force-update")]
    public async Task<IActionResult> ForceUpdate()
    {
        var client = _httpClientFactory.CreateClient();
        var baseUrl = _configuration["Services:CurrencyRatesService"] ?? "http://currency-rates-service:8080";
        
        var response = await client.GetAsync($"{baseUrl}/api/Admin/ForceUpdateRates");
        
        if (response.IsSuccessStatusCode)
        {
            return Ok(new { message = "Update triggered successfully" });
        }

        return StatusCode((int)response.StatusCode, "Failed to trigger update in CurrencyRatesService");
    }
}
