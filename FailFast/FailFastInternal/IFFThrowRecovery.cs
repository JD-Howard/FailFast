namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

public interface IFFThrowRecovery
{
    /// <summary>
    /// True if the expression provided to FailFast.When.Throws() actually threw an exception.
    /// </summary>
    public bool Result { get; }
    
    
    /// <summary>
    /// Use this to execute code that should recover from an FailFast.When.Throws() exception.
    /// This does not execute Action unless Throws() threw an exception and does not have a Try/Catch.
    /// OnException() expression should either be bullet proof or otherwise intended to throw. 
    /// </summary>
    /// <param name="expression">Method that performs any error recovery actions</param>
    /// <returns>True/False (passthrough) from the original Throws() expression</returns>
#if RELEASE
#endif
    bool OnException(Action expression);

    
    /// <inheritdoc cref="IFFThrowRecovery.OnException(Action)"/>
    /// <param name="expression">Method that receives the exception context and performs desired recovery actions</param>
#if RELEASE
#endif
    bool OnException(Action<Exception> expression);

    
    /// <summary>
    /// Use the 'On&lt;T&gt;' method to handle very specific exceptions. This is a fluent interface so you can create custom
    /// recovery logic for a variety of different expected exceptions; which the best practice vs only using the OnException
    /// catch-all. The return type is a notable difference, but you can just access the 'Result' property if you need a bool. 
    /// </summary>
    /// <param name="expression">Method that receives the exception context and performs desired recovery actions</param>
    /// <typeparam name="T">Must derive from Exception</typeparam>
    /// <returns>IFFThrowRecovery, for chaining multiple 'On&lt;T&gt;' handlers</returns>
#if RELEASE
#endif
    IFFThrowRecovery On<T>(Action<T> expression) where T : Exception;
}