using System.Collections;
using System.Diagnostics.CodeAnalysis.FailFastInternal;

namespace UnitTests;

public class SetupConfig : IFailFastConfig
{
    private static SetupConfig _instance;
    public static SetupConfig GetMock() => _instance ??= new SetupConfig();

    private SetupConfig() { }
    
    public int LogCount { get; set; }


    public bool GetCanDebugBreak() => false;
    public void SetCanDebugBreak(bool authState){}
    

    public void FailFastBreak(string caller, string routing, object? context) => LogCount++;

    public void FailFastThrow(string caller, Exception error) => LogCount++;
}