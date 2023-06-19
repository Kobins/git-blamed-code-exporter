namespace GitBlamedCodeExporter;

public class Atomics
{
    public static uint Count = 0;
    
    public static void Increase()
    {
        Interlocked.Increment(ref Count);
    }
    public static void Decrease()
    {
        Interlocked.Decrement(ref Count);
    }
}