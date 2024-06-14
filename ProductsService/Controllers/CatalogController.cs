using Microsoft.AspNetCore.Mvc;
using ProdPlace.Interfaces;

namespace ProdPlace.Controllers;

[Route("catalog")]
public class CatalogController : Controller
{
    private ICatalogController _catalogController;

    public CatalogController(ICatalogController catalogController)
    {
        _catalogController = catalogController;
    }

    // GET
    public IActionResult Index()
    {
        return View();
    }
}