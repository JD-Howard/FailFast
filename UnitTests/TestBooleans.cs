namespace UnitTests
{
    public class TestBooleans
    {

        [Test]
        public void WhenTrueExpectTrue()
        {
            Assert.IsTrue(FailFast.When.True(true));
        }
        
        [Test]
        public void WhenTrueExpectFalse()
        {
            Assert.IsFalse(FailFast.When.True(false));
            Assert.IsFalse(FailFast.When.True(null));
        }
        
        [Test]
        public void WhenNotTrueExpectTrue()
        {
            Assert.IsTrue(FailFast.When.NotTrue(false));
            Assert.IsTrue(FailFast.When.NotTrue(null));
        }
        
        [Test]
        public void WhenNotTrueExpectFalse()
        {
            Assert.IsFalse(FailFast.When.NotTrue(true));
        }
        
        
        [Test, NonParallelizable]
        public void BooleanLogHits()
        {
            Setup.LogCount = 0;
            FailFast.When.True(true);
            FailFast.When.True(false);
            FailFast.When.NotTrue(true);
            FailFast.When.NotTrue(false);
            Assert.AreEqual(2, Setup.LogCount);
        }
        
    }
}