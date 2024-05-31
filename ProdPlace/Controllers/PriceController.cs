using Microsoft.AspNetCore.Mvc;

namespace ProdPlace.Controllers;

[Route("price")]
public class PriceController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Update(int productId, double price)
    {
        return Ok();
    }
}