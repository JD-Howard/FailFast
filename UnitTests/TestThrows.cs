namespace UnitTests;

public class TestThrows
{

    public void ThrowYes() => ThrowMaybe(0); // Causes divide by 0 error

    public void ThrowNo() => ThrowMaybe(2);

    public void ThrowMaybe(int by)
    {
        var result = 2 / by;
    }


    [Test]
    public void WhenThrowsNoArgsExpectTrue()
    {   
        Assert.IsTrue(FailFast.When.Throws(ThrowYes).Result);
        Assert.IsTrue(FailFast.When.Throws(() =>
        {
            ThrowYes();
        }).Result);
    }
    
    [Test]
    public void WhenThrowsNoArgsExpectFalse()
    {   
        Assert.IsFalse(FailFast.When.Throws(ThrowNo).Result);
        Assert.IsFalse(FailFast.When.Throws(() =>
        {
            ThrowNo();
        }).Result);
    }
    
        
    [Test, NonParallelizable]
    public void ThrowLogHits()
    {
        Global.LogCount = 0;
        FailFast.When.Throws(ThrowYes);
        FailFast.When.Throws(ThrowNo);
        FailFast.When.Throws(() => ThrowMaybe(0));
        FailFast.When.Throws(() => ThrowMaybe(2));
        Assert.AreEqual(2, Global.LogCount);
    }
    
}