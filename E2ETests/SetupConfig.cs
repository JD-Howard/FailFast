using System.Collections;
using System.Diagnostics.CodeAnalysis.FailFastInternal;

namespace E2ETests;

public class SetupConfig : IFailFastConfig
{
    private static SetupConfig? _instance;
    public static SetupConfig GetMock() => _instance ??= new SetupConfig();

    private SetupConfig() { }
    
    public int LogCount { get; set; }
    public bool CanDebugBreak { get; set; } = true;

    public void FailFastBreak(string caller, Guid? groupId, string routing, IEnumerable? context) => LogCount++;

    public void FailFastThrow(string caller, Exception error) => LogCount++;
}