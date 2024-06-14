using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProdPlace.db;
using ProdPlace.Models;

namespace ProdPlace.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        /*
        var products = dbContext.Products;
        foreach (var p in products)
        {
            Console.WriteLine(p.Id);
            Console.WriteLine(p.Name);
            Console.WriteLine(p.Description);
            Console.WriteLine(p.Price);
            Console.WriteLine(new string('=', 30));
        }
        */

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}