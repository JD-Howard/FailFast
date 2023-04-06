namespace UnitTests;

[SetUpFixture]
public class Global
{
    public static int LogCount { get; set; }
    
    public static Exception? Catcher(Action work)
    {
        try { 
            work(); 
            return null; 
        }
        catch (Exception e) { return e; }
    }
        
    [OneTimeSetUp]
    public void SetUp()
    {
        // Debugger.Break() can't be unit tested, see IntegrationTests & E2ETests
        FailFast.Initialize(FFBreakOption.InitFalse, 
                            Catcher, 
                            (x, y, z) => LogCount++, 
                            x => LogCount++);
    }

    [OneTimeTearDown]
    public void TearDown() { }
}
