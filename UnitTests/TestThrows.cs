namespace UnitTests;

public class TestThrows
{

    public void ThrowYes() => ThrowMaybe(0); // Causes divide by 0 error

    public void ThrowNo() => ThrowMaybe(2);

    public void ThrowMaybe(int by)
    {
        var result = 2 / by;
    }

    [SetUp]
    public void Setup()
    {
        // Debugger.Break() can't be unit tested, see E2ETests.ExplicitTests
        FailFast.Initialize(SetupConfig.GetMock());
    }

    [Test]
    public void WhenThrowsNoArgsExpectTrue()
    {   
        Assert.IsTrue(FailFast.When.Throws(ThrowYes));
        Assert.IsTrue(FailFast.When.Throws(() => ThrowMaybe(0)));
    }
    
    [Test]
    public void WhenThrowsNoArgsExpectFalse()
    {   
        Assert.IsFalse(FailFast.When.Throws(ThrowNo));
        Assert.IsFalse(FailFast.When.Throws(() => ThrowMaybe(2)));
    }
    
    [Test]
    public void WhenThrowsWithArgsExpectTrue()
    {   
        Assert.IsTrue(FailFast.When.Throws(0, ThrowMaybe));
    }
    
    [Test]
    public void WhenThrowsWithArgsExpectFalse()
    {   
        Assert.IsFalse(FailFast.When.Throws(2, ThrowMaybe));
    }
        
    [Test, NonParallelizable]
    public void ThrowLogHits()
    {
        SetupConfig.GetMock().LogCount = 0;
        FailFast.When.Throws(ThrowYes);
        FailFast.When.Throws(ThrowNo);
        FailFast.When.Throws(0, ThrowMaybe);
        FailFast.When.Throws(2, ThrowMaybe);
        Assert.AreEqual(2, SetupConfig.GetMock().LogCount);
    }
    
}