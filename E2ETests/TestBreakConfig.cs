namespace E2ETests;

public class TestBreakConfig
{
    
    [SetUp]
    public void Setup()
    {
        // These tests all invoke Debugger.Break() and must be run manually
        FailFast.Initialize(SetupConfig.GetMock());
    }


    private void BreakOn() => SetupConfig.GetMock().SetCanDebugBreak(true);
    private void BreakOff() => SetupConfig.GetMock().SetCanDebugBreak(false);
    private void Reset() => SetupConfig.GetMock().LogCount = 0;
    private int LogCount => SetupConfig.GetMock().LogCount;
        
    
    [Test, Explicit("DEBUG this test and see inline notes for expectations")]
    public void WhenForDebug()
    {
        // Set project to run in Debug mode
        
        Reset();
        BreakOff();
        
        // should not cause break
        Assert.IsTrue(FailFast.When.True(1 == 1));
        
        BreakOn();
        
        // should cause break
        Assert.IsTrue(FailFast.When.True(1 == 1));
        
        // "When" should have generated logs regardless of whether Breaking was turned on
        Assert.AreEqual(2, LogCount);
    }
    
    
    [Test, Explicit("RUN this test and it should complete quickly never hitting a Debugger.Break()")]
    public void WhenForRelease()
    {
        // Set project to run in RELEASE mode
        
        Reset();
        BreakOff();
        
        // should not cause break
        Assert.IsTrue(FailFast.When.True(1 == 1));
        
        BreakOn();
        
        // should cause break while in Debug mode, but not in release
        Assert.IsTrue(FailFast.When.True(1 == 1));
        
        // When should have generated logs regardless of whether Breaking was turned on
        Assert.AreEqual(2, LogCount);
    }
    
    
}