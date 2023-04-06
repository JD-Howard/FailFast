using System;
using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

internal readonly struct FFThrowRecovery : IFFThrowRecovery
{
    private readonly bool _result;
    private readonly int _exceptionToken;
    private readonly bool _handled;


    /// <summary>
    /// Completes the FailFast.When.Throws() chain for handling thrown exceptions. 
    /// </summary>
    /// <param name="result">Result of the original Throws() method</param>
    /// <param name="errToken">If Throws() generated an exception, use FF.FindException(errToken) to retrieve</param>
    /// <param name="handled">Set to true if an On&lt;T&gt; handler matched the type of exception</param>
    internal FFThrowRecovery(bool result, int errToken, bool handled = false)
    {
        _handled = handled;
        _result = result;
        _exceptionToken = errToken;
    }

    /// <inheritdoc cref="IFFThrowRecovery.Result"/>
    public bool Result => _result;


    /// <inheritdoc cref="IFFThrowRecovery.OnException(Action)"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool OnException(Action expression)
    {
        switch (_result)
        {
            case false:
                return false;
            case true:
                if (_handled)
                    return true;
                
                expression();
                return true;
        }
    }
    
    
    /// <inheritdoc cref="IFFThrowRecovery.OnException(Action&lt;Exception&gt;)"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool OnException(Action<Exception> expression)
    {
        switch (_result)
        {
            case false:
                return false;
            case true:
                if (_handled)
                    return true;
                
                if (FailFast.FindException(_exceptionToken) is Exception ex)
                    expression(ex);
                
                return true;
        }
    }


    /// <inheritdoc cref="IFFThrowRecovery.On&lt;Exception&gt;"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public IFFThrowRecovery On<T>(Action<T> expression) where T : Exception
    {   
        // TODO: Should handled exceptions affect the result and/or logging strategy?
        //       Not a fan of shoving it down the chain and potentially missing log opportunities...
        
        // POI: less conventional, but acceptable way of rethrowing exceptions at later stages of execution.
        // https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions#capture-exceptions-to-rethrow-later
        
        if (FailFast.FindException(_exceptionToken) is T exception)
        {
            expression(exception);
            return new FFThrowRecovery(_result, _exceptionToken, true);
        }

        return this;
    }

}