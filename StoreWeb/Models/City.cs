namespace StoreWeb.Models;

public class City
{
    public int Number { get; set; }
    public String Name { get; set; }

    public override string ToString()
    {
        return $"{Name} ({Number})";
    }
}