namespace UnitTests;

public class SetupDummy
{
    public string Name { get; }
    public int Value { get; }

    public SetupDummy(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public static SetupDummy? GetInstance(bool create = true) 
        => create ? new SetupDummy("Test", 1) : null;

}