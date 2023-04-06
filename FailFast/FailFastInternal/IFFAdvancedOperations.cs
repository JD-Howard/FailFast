namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

public interface IFFAdvancedOperations : IFFPrimitiveOperations
{
    /// <summary>
    /// Executes a provided method/lambda in a Try/Catch and resolves to true if an exception is thrown.
    /// </summary>
    /// <param name="expression">A method or lambda that does something that may throw an exception</param>
    /// <returns>FFThrowRecovery can resolve to boolean or be used to execute OnError (recovery) code</returns>
    IFFThrowRecovery Throws(Action expression);
    
}