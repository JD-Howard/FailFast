using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

internal readonly struct FailFastOperations : IFailFastOperations
{
    
    private readonly FailFastContext _context;

    public FailFastOperations(FailFastContext context) => _context = context;

    
    
    /// <inheritdoc cref="IFailFastOperations.Null"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool Null([NotNullWhen(false)] object? obj, [CallerMemberName] string caller = "")
    {
        if (obj is not null)
            return false;
        
        if (_context != FailFastContext.ExplicitBreak)
            FailFast.Config?.FailFastBreak(caller, null, "FailFast.When.Null", new[] {obj});
        
        if (_context != FailFastContext.NoBreak && Debugger.IsAttached)
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFailFastOperations.NotNull"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool NotNull([NotNullWhen(true)] object? obj, [CallerMemberName] string caller = "")
    {
        if (obj is null)
            return false;
        
        if (_context != FailFastContext.ExplicitBreak)
            FailFast.Config?.FailFastBreak(caller, null, "FailFast.When.NotNull", new[] {obj});
        
        if (_context != FailFastContext.NoBreak && Debugger.IsAttached)
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFailFastOperations.True"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool True(bool? test, [CallerMemberName] string caller = "")
    {
        if (true != test)
            return false;
        
        if (_context != FailFastContext.ExplicitBreak)
            FailFast.Config?.FailFastBreak(caller, null, "FailFast.When.True", new[] {test});
        
        if (_context != FailFastContext.NoBreak && Debugger.IsAttached)
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFailFastOperations.NotTrue"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool NotTrue(bool? test, [CallerMemberName] string caller = "")
    {
        if (true == test)
            return false;
        
        if (_context != FailFastContext.ExplicitBreak)
            FailFast.Config?.FailFastBreak(caller, null, "FailFast.When.NotTrue", new[] {test});
        
        if (_context != FailFastContext.NoBreak && Debugger.IsAttached)
            Debugger.Break();

        return true;
    }
    
    
    /// <inheritdoc cref="IFailFastOperations.Throws"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool Throws(Action expression, [CallerMemberName] string caller = "")
    {
        try
        {
            expression();
            return false;
        }
        catch (Exception e)
        {
            if (_context != FailFastContext.ExplicitBreak)
                FailFast.Config?.FailFastThrow(caller, e);
        
            if (_context != FailFastContext.NoBreak && Debugger.IsAttached)
                Debugger.Break();
            
            return true;
        }
    }

    /// <inheritdoc cref="IFailFastOperations.Throws&lt;T&gt;"/>
#if RELEASE
    [DebuggerHidden]
#endif
    public bool Throws<T>(T args, Action<T> expression, [CallerMemberName] string caller = "")
    {
        try
        {
            expression(args);
            return false;
        }
        catch (Exception e)
        {
            if (_context != FailFastContext.ExplicitBreak)
                FailFast.Config?.FailFastThrow(caller, e);
        
            if (_context != FailFastContext.NoBreak && Debugger.IsAttached)
                Debugger.Break();
            
            return true;
        }
    }

}