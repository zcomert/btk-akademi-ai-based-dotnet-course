namespace StoreWeb.Models;

public class GreetingModel
{
    public String? Name { get; set; }
    public DateTime Date => DateTime.Now;
    public List<String>? Cities { get; set; }
    public GreetingModel()
    {
        Cities = new List<String>();
    }

   
}