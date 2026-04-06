using Microsoft.AspNetCore.Mvc;
using StoreWeb.Models;

namespace StoreWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Greeting(String name)
    {
        var model = new GreetingModel
        {
            Name = name
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