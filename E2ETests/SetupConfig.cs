using System.Collections;
using System.Diagnostics.CodeAnalysis.FailFastInternal;

namespace E2ETests;

public class SetupConfig : IFailFastConfig
{
    private static SetupConfig? _instance;
    public static SetupConfig GetMock() => _instance ??= new SetupConfig();

    private SetupConfig() { }
    
    public int LogCount { get; set; }

    
    private bool _canBreak = true;
    public bool GetCanDebugBreak() => _canBreak;
    public void SetCanDebugBreak(bool authState) => _canBreak = authState;


    public void FailFastBreak(string caller, string routing, object? context) => LogCount++;

    public void FailFastThrow(string caller, Exception error) => LogCount++;
}