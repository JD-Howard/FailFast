namespace E2ETests;

public class TestBreakExplicit
{
    
    [SetUp]
    public void Setup()
    {
        // These tests all invoke Debugger.Break() and must be run manually
        FailFast.Initialize(SetupConfig.GetMock());
    }
    
    
    private void BreakOff() => SetupConfig.GetMock().SetCanDebugBreak(false);
    private void Reset() => SetupConfig.GetMock().LogCount = 0;
    private int LogCount => SetupConfig.GetMock().LogCount;
        
    
    // Note 
    [Test, Explicit("DEBUG this test and see inline notes for expectations")]
    public void ExplicitBreakWhenForDebug()
    {
        // Set project to run in DEBUG mode
        
        Reset();
        BreakOff();
        
        // should break even though CanDebugBreak is turned off
        Assert.IsTrue(FailFast.BreakWhen.True(1 == 1));
        
        // BreakWhen should never generate logs
        Assert.AreEqual(0, LogCount);
    }
    
    
    [Test, Explicit("RUN this test and it should complete quickly never hitting a Debugger.Break()")]
    public void ExplicitBreakWhenForRelease()
    {
        // Set project to run in RELEASE mode
        
        Reset();
        BreakOff();

        // IF RUN in Debug mode
        //      Throws because FastFail DLL is a release build and RUNing this does not have an attached debugger
        // IF RUN in Release mode
        //      Throws because FastFail DLL is a release build, no DEBUG EV defined AND no an attached debugger
        // Note: We have to try debugging releases sometimes
        //      So, a Release build + a debugger is a valid configuration for hitting Debugger.Break()
        Assert.Throws<InvalidOperationException>(() =>
        {
            FailFast.BreakWhen.True(1 == 1);
        });
        
        // BreakWhen should never generate logs
        Assert.AreEqual(0, LogCount);
    }
        
        
}