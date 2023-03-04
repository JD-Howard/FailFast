using System.Diagnostics;

namespace UnitTests
{
    public class TestNullables
    {
        SetupDummy? valueStub = SetupDummy.GetInstance();
        SetupDummy? nullStub = SetupDummy.GetInstance(false);
        
        [SetUp]
        public void Setup()
        {
            // Debugger.Break() can't be unit tested, see E2ETests.ExplicitTests
            FailFast.Initialize(SetupConfig.GetMock());
        }

        [Test]
        public void WhenNullExpectTrue()
        {   
            Assert.IsTrue(FailFast.When.Null(nullStub));
        }
        
        [Test]
        public void WhenNullExpectFalse()
        {
            Assert.IsFalse(FailFast.When.Null(valueStub));
        }
        
        [Test]
        public void WhenNotNullExpectTrue()
        {   
            Assert.IsTrue(FailFast.When.NotNull(valueStub));
        }
        
        [Test]
        public void WhenNotNullExpectFalse()
        {
            Assert.IsFalse(FailFast.When.NotNull(nullStub));
        }
        
        [Test, NonParallelizable]
        public void NullableLogHits()
        {
            SetupConfig.GetMock().LogCount = 0;
            FailFast.When.Null(valueStub);
            FailFast.When.Null(nullStub);
            FailFast.When.NotNull(valueStub);
            FailFast.When.NotNull(nullStub);
            Assert.AreEqual(2, SetupConfig.GetMock().LogCount);
        }
        
        
    }
}