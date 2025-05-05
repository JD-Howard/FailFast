namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        internal readonly struct Operations : IAdvancedOps
        {
            private readonly FFContext _context;

            [DebuggerHidden]
            internal Operations(FFContext context) => _context = context;


            /// <inheritdoc cref="IPrimitiveOps.Null"/>
            [DebuggerHidden]
            public bool Null([NotNullWhen(false)] object? obj, [CallerMemberName] string caller = "")
            {
                if (obj != null)
                    return false;

                if (_context.HasFlag(FFContext.Log) && Configure.BreakLogHandler != null)
                    Configure.BreakLogHandler(caller, "FailFast.When.Null", obj);

                if (_context.HasFlag(FFContext.Break))
                    Debugger.Break();

                return true;
            }


            /// <inheritdoc cref="IPrimitiveOps.NotNull"/>
            [DebuggerHidden]
            public bool NotNull([NotNullWhen(true)] object? obj, [CallerMemberName] string caller = "")
            {
                if (obj is null)
                    return false;

                if (_context.HasFlag(FFContext.Log) && Configure.BreakLogHandler != null)
                    Configure.BreakLogHandler(caller, "FailFast.When.NotNull", obj);

                if (_context.HasFlag(FFContext.Break))
                    Debugger.Break();

                return true;
            }


            /// <inheritdoc cref="IPrimitiveOps.True"/>
            [DebuggerHidden]
            public bool True(bool? test, [CallerMemberName] string caller = "")
            {
                if (true != test)
                    return false;

                if (_context.HasFlag(FFContext.Log) && Configure.BreakLogHandler != null)
                    Configure.BreakLogHandler(caller, "FailFast.When.True", test);

                if (_context.HasFlag(FFContext.Break))
                    Debugger.Break();

                return true;
            }


            /// <inheritdoc cref="IPrimitiveOps.NotTrue"/>
            [DebuggerHidden]
            public bool NotTrue(bool? test, [CallerMemberName] string caller = "")
            {
                if (true == test)
                    return false;

                if (_context.HasFlag(FFContext.Log))
                    Configure.BreakLogHandler?.Invoke(caller, "FailFast.When.NotTrue", test);

                if (_context.HasFlag(FFContext.Break))
                    Debugger.Break();

                return true;
            }


            /// <inheritdoc cref="IAdvancedOps.Throws"/>
            [DebuggerHidden]
            public IThrowRecovery Throws(Action expression, [CallerMemberName] string caller = "")
            {
                var ex = ThrowsCatcher(expression);
                if (ex is null)
                    return new ThrowRecovery(false, -1, true);

                var token = GetExceptionToken(ex);
                if (_context.HasFlag(FFContext.Log))
                    Configure.ThrowsLogHandler?.Invoke(caller, ex);

                if (_context.HasFlag(FFContext.Break))
                    Debugger.Break();
                
                return new ThrowRecovery(true, token, false);
            }
        }
        
    }
}