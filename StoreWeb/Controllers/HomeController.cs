using Microsoft.AspNetCore.Mvc;

namespace StoreWeb.Controllers;

public class HomeController : Controller
{
    public String Index()
    {
        return "Hello World";
    }

    public String Greeting(String name)
    {
        return $"Merhaba {name}";
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