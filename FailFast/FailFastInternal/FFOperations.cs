using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

internal readonly struct FFOperations : IFFAdvancedOperations
{
    
    private readonly FFContext _context;

    public FFOperations(FFContext context) => _context = context;

    
    /// <inheritdoc cref="IFFPrimitiveOperations.Null"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool Null([NotNullWhen(false)] object? obj, [CallerMemberName] string caller = "")
    {
        if (obj is not null)
            return false;
        
        if (_context.HasFlag(FFContext.Log))
            FailFast.Config.BreakLogHandler(caller, "FailFast.When.Null", obj);
        
        if (_context.HasFlag(FFContext.Break))
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFFPrimitiveOperations.NotNull"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool NotNull([NotNullWhen(true)] object? obj, [CallerMemberName] string caller = "")
    {
        if (obj is null)
            return false;
        
        if (_context.HasFlag(FFContext.Log))
            FailFast.Config.BreakLogHandler(caller, "FailFast.When.NotNull", obj);
        
        if (_context.HasFlag(FFContext.Break))
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFFPrimitiveOperations.True"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool True(bool? test, [CallerMemberName] string caller = "")
    {
        if (true != test)
            return false;
        
        if (_context.HasFlag(FFContext.Log))
            FailFast.Config.BreakLogHandler(caller, "FailFast.When.True", test);
        
        if (_context.HasFlag(FFContext.Break))
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFFPrimitiveOperations.NotTrue"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool NotTrue(bool? test, [CallerMemberName] string caller = "")
    {
        if (true == test)
            return false;
        
        if (_context.HasFlag(FFContext.Log))
            FailFast.Config.BreakLogHandler(caller, "FailFast.When.NotTrue", test);
        
        if (_context.HasFlag(FFContext.Break))
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFFAdvancedOperations.Throws"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public IFFThrowRecovery Throws(Action expression)
    {
        var ex = FailFast.Config.TryCatchHandler(expression);
        if (ex is null)
            return new FFThrowRecovery(false, -1);

        if (_context.HasFlag(FFContext.Log))
            FailFast.Config.ThrowsLogHandler(ex);

        if (_context.HasFlag(FFContext.Break))
            Debugger.Break();

        var token = FailFast.GetExceptionToken(ex);
        return new FFThrowRecovery(true, token);
    }


}