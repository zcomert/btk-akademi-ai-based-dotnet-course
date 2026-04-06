namespace StoreWeb.Models;

public class GreetingModel
{
    public String? Name { get; set; }
    public DateTime Date => DateTime.Now;
    public List<City>? Cities { get; set; }
    public GreetingModel()
    {
        Cities = new List<City>();
    }

   
}
