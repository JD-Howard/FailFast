global using NUnit.Framework;
global using System.Diagnostics;

namespace UnitTests;

[SetUpFixture]
public class Setup
{
    public static int LogCount { get; set; }
    
        
    [OneTimeSetUp]
    public void SetUp()
    {
        // Debugger.Break() cannot be unit tested, see IntegrationTests for those
        FailFast.Configure.BreakBehavior = FailFast.FFBreakOption.InitFalse;
        FailFast.Configure.BreakLogHandler = (x, y, z) => LogCount++;
        FailFast.Configure.ThrowsLogHandler = (x, y) => LogCount++;
    }

    [OneTimeTearDown]
    public void TearDown() { }
}
