using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () =>
{
    String s = "Hello World";
    return s;
});

app.MapGet("/greeting/{name}", (String name) => $"Merhaba {name}");

app.MapGet("/contact", () => "İletişim");

app.MapGet("/products", (Int16 q=0) =>
{
    String result = "";
    int i = 0;
    while (i < q)
    {
        result += $"{i}\n";
        i = i + 2;
    }
    return result;
});


app.MapGet("/calc/{a:int}/{b:int}", ([FromRoute(Name = "a")] Int16 a,
[FromRoute(Name = "b")] Int16 b,
[FromQuery(Name = "q")] String q) => q is null 
        ? $"{a}*{b}={a*b}"
        : $"Bak {q} {a}*{b}={a*b}"
);

app.Run();
