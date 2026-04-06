using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models;

namespace StoreWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Greeting(String id)
    {
        var model = new GreetingModel
        {
            Name = id,
            Cities = new List<City> { 
                new City { Number = 55, Name = "Samsun" },
                new City { Number = 6, Name = "Ankara" },
                new City { Number = 34, Name = "İstanbul" },
                new City { Number = 35, Name = "İzmir" }
            }
        };

        return View(model);
    }

    public String Contact()
    {
        return "İletişim";
    }

    public String Products(Int16 q)
    {
        String result = "";
        int i = 0;
        while (i < q)
        {
            result += $"{i}\n";
            i = i + 2;
        }
        return result;
    }

    public String Calc(int a, int b, String q)
    {
        return q is null
            ? $"{a}*{b}={a * b}"
            : $"Bak {q} {a}*{b}={a * b}";
    }
}