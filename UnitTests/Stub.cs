namespace UnitTests;

public class Stub
{
    public string Name { get; }
    public int Value { get; }

    public Stub(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public static Stub? GetInstance(bool create = true) 
        => create ? new Stub("Test", 1) : null;

}