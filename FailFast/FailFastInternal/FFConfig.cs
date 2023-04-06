using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

internal class FFConfig
{
    private readonly FFBreakOption _canBreakBehavior;
    private bool _canBreak;

    internal bool IsDefault { get; }

    /// <inheritdoc cref="FailFast.CanDebugBreak"/>
    internal bool CanDebugBreak
    {
        get => _canBreak;
        set => _canBreak = _canBreakBehavior < FFBreakOption.StaticFalse ? value : _canBreak;
    }
    
    internal FailFast.FFLogBreak BreakLogHandler { get; }
    
    internal FailFast.FFLogThrow ThrowsLogHandler { get; }
    
    internal FailFast.FFTryCatch TryCatchHandler { get; }


    /// <summary>
    /// This constructor is only used as a fallback if FailFast.Initialize() was never executed.
    /// </summary>
    internal FFConfig()
    {
        IsDefault = true;
        _canBreakBehavior = FFBreakOption.StaticFalse;
        _canBreak = false;
        BreakLogHandler = DefaultLogBreak;
        ThrowsLogHandler = DefaultLogThrow;
        TryCatchHandler = DefaultTryCatch;
    }
    
    /// <summary>
    /// Used by FailFast.Initialize()
    /// </summary>
    [DebuggerHidden, DebuggerStepThrough]
    internal FFConfig(FFBreakOption breakOption, FailFast.FFLogBreak? breakHandler, 
                      FailFast.FFLogThrow? throwsHandler, FailFast.FFTryCatch? tryCatchHandler)
    {
        IsDefault = false;
        _canBreakBehavior = breakOption;
        _canBreak = breakOption switch
        {
            FFBreakOption.StaticTrue => true,
            FFBreakOption.InitTrue => true,
            _ => false
        };

        BreakLogHandler = breakHandler ?? DefaultLogBreak;
        ThrowsLogHandler = throwsHandler ?? DefaultLogThrow;
        
        
        if (tryCatchHandler is null)
            throw new ArgumentNullException(nameof(tryCatchHandler), "The FailFast.Initialize() tryCatchHandler cannot be null");
        
        TryCatchHandler = tryCatchHandler;
    }

    
    
    
    
    
    
    /// <inheritdoc cref="FailFast.FFLogBreak"/>
    [DebuggerHidden]
    private static void DefaultLogBreak(string caller, string routing, object? context)
    {
        Debug.WriteLine($"FailFast Break By: {caller}, Path: {routing}, Context: {context}");
    }
    
    /// <inheritdoc cref="FailFast.FFLogThrow"/>
    [DebuggerHidden]
    private static void DefaultLogThrow(Exception error)
    {
        Debug.WriteLine($"FailFast Throws Error: {error.Message}, Stack: {error.StackTrace}");
    }
    
    /// <inheritdoc cref="FailFast.FFTryCatch"/>
    [DebuggerHidden]
    private static Exception? DefaultTryCatch(Action expression)
    {
        // Note: This method only fulfills the requirement, it does not behave properly
        //       unless the Try/Catch is inside of the consumer code base.
        try
        {
            expression();
            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }
    
}