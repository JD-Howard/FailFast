using System.Collections;

namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

/// <summary>
/// Used to create a Log capturing point.
/// Note that only code using TryBreak() &amp; Throws() will generate log data.
/// </summary>
public interface IFailFastConfig
{   
    public bool GetCanDebugBreak();
    public void SetCanDebugBreak(bool authState);
    
    /// <summary>
    /// Captures the presence of a bad result regardless of whether CanDebuggerBreak=True.
    /// This could be a lot more logs than desired and you should implement required filtering.
    /// If desired, your implementation should generate the StackTrace/rewind frames as needed.
    /// </summary>
    /// <param name="caller">Method that invoked TryBreak()</param>
    /// <param name="result">The last IResult produced before the TryBreak() was called</param>
    public void FailFastBreak(string caller, string routing, object? context);
    
    /// <summary>
    /// Captures exception data from the Throws() method
    /// </summary>
    /// <param name="caller">Method that invoked Throws()</param>
    /// <param name="error">The exception that was thrown in the FailFast TryCatch block</param>
    public void FailFastThrow(string caller, Exception error);
}