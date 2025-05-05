namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        /// <summary>
        /// Used internally for behavior, but also determines if "FailFast.When" can execute
        /// Debugger.Break() calls, and can optionally define "bad result" log capabilities.
        /// </summary>
        internal static class Configure
        {
            private static object _breakBehaviorLock = new object();
            
        #if DEBUG // These are considered to be the safe defaults
            private static volatile FFBreakOption _canBreakBehavior = FFBreakOption.InitFalse;
        #else 
            private static volatile FFBreakOption _canBreakBehavior = FFBreakOption.StaticFalse;
        #endif

            /// <summary>
            /// Determines if Debugger.Break() is allowed to fire. If using Init[False|True]
            /// then you can actually activate/toggle this in the code section you specifically
            /// want to inspect. However, once a Static[False|True] version is activated, then
            /// you will no longer be able to toggle this option. A StaticFalse value should
            /// be used for all production builds, but Debugger.Break() is prevented whenever
            /// a debugger is not attache; which should prevent side effects even if your
            /// demonstrating an internal debug build for someone. 
            /// </summary>
            public static FFBreakOption BreakBehavior
            {
                get { lock (_breakBehaviorLock) { return _canBreakBehavior; } }
                set => TrySetBreakBehavior(value);
            }

            private static bool TrySetBreakBehavior(FFBreakOption option)
            {
                lock (_breakBehaviorLock)
                {
                    if (_canBreakBehavior >= FFBreakOption.StaticFalse)
                        return false; // If ever configured to a static value, then it cannot be changed at runtime

            #if DEBUG
                    var hasDebugger = Debugger.IsAttached;
                    if (hasDebugger) // Only allow runtime changes if debugger is attached
                        _canBreakBehavior = option;
                    return hasDebugger;
            #else
                return false;
            #endif
                }
            }



            /// <summary>
            /// Determines whether FailFast will invoke Debugger.Break() when a condition resolve to true.
            /// </summary>
            internal static bool CanDebugBreakTest 
                => (_canBreakBehavior == FFBreakOption.InitTrue || _canBreakBehavior == FFBreakOption.StaticTrue) && Debugger.IsAttached;
            
            
            /// <summary>
            /// Determines whether FailFast has any configured logging capabilities.
            /// </summary>
            internal static bool CanLogTest 
                => BreakLogHandler != null || ThrowsLogHandler != null;
            
            
            
            /// <summary>
            /// Captures the presence of a bad result regardless of whether CanDebuggerBreak=True.
            /// This could be a lot more logs than desired and you should implement required filtering.
            /// If desired, your implementation should generate the StackTrace/rewind frames as needed.
            /// optional log entry point for non-Throws() methods that resolve to true
            /// </summary>
            /// <param name="caller">Method name that invoked FailFast</param>
            /// <param name="routing">The FailFast specific path syntax executed</param>
            /// <param name="context">The object or boolean that was passed to a FailFast method</param>
            internal delegate void FFLogBreak(string caller, string routing, object? context);
            
            internal static FFLogBreak? BreakLogHandler { get; set; } = null;
            
            
            
            /// <summary>
            /// Captures exception data from the Throws() method
            /// optional log entry point for Throws() if it produced an exception
            /// </summary>
            /// <param name="caller">Method name that invoked FailFast</param>
            /// <param name="error">The exception that was thrown in the FailFast TryCatch block</param>
            internal delegate void FFLogThrow(string caller, ExceptionDispatchInfo error);
            
            internal static FFLogThrow? ThrowsLogHandler { get; set; } = null;
            
            
        }
        
        
    }
}