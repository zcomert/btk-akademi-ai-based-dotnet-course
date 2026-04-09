namespace ConsoleExtensionApp.Extension;
public static class MathCalc
{
    public static int Factorial(this int n)
    {
        if (n == 0)
            return 1;
        else
            return n * Factorial(n - 1);
    }
}


