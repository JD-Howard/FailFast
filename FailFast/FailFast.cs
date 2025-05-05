namespace System.Diagnostics
{
    
    internal static partial class FailFast
    {


        /// <summary>
        /// The Debugger.Break() permissions are delegated to the FailFast Configuration.
        /// All operations resolving to True will attempt to generate logs.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if FailFast.Initialize() was not properly pre-configured</exception>
        [DebuggerHidden]
        internal static IAdvancedOps When 
        {
            get
            {
                var context = FFContext.None;
                if (Configure.CanLogTest)
                    context |= FFContext.Log;
                
                if (Configure.CanDebugBreakTest)
                    context |= FFContext.Break;
                
                return new Operations(context);             
            }
            
        }
        
        
#if DEBUG // Non-debug builds should throw not-defined errors if FailFast.BreakWhen is being used
        
        /// <summary>
        /// Does not require configuration, but never produces logs and will not provide the Throws() routing.
        /// This is NOT for production builds &amp; always forces a Debugger.Break() if the operation resolves to True + debugger is attached.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if called and the DEBUG flag is not defined</exception>
        [DebuggerHidden]
        internal static IPrimitiveOps BreakWhen
        {
            get
            {
                var hasDebugDirective = false;
                UpdateDebugStatus(ref hasDebugDirective);

                if (!hasDebugDirective)
                    throw new InvalidOperationException("FailFast.BreakWhen statements should only exist in DEBUG builds");
                
                // Do not break without debugger. The Debugger.Break() has an effect even without an attached debugger.
                // This defensive strategy is good for scenarios where a debug build is being used as an internal demo.
                return new Operations(Debugger.IsAttached ? FFContext.Break : FFContext.None);

            }
        }
#endif
                

        /// <summary>
        /// This should only ever executes if the DEBUG environment variable is defined.
        /// Kind of a round about way to help consumers avoid volatile side effects.
        /// </summary>
        [Conditional("DEBUG")]
        private static void UpdateDebugStatus(ref bool isDebugMode) => isDebugMode = true;
        
        
        
        #region Exception Token Handling

        [DebuggerHidden]
        private static ExceptionDispatchInfo? ThrowsCatcher(Action expression)
        {
            try
            {
                expression();
                return null;
            }
            catch (Exception e)
            {
                // https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions#capture-exceptions-to-rethrow-later
                return ExceptionDispatchInfo.Capture(e);
            }
        }
        
        private static readonly object ExceptionDbLock = new object();
        private static readonly ExceptionDispatchInfo[] ExceptionDb = new ExceptionDispatchInfo[100];
        private static int NextIndex = 0;
    
        [DebuggerHidden]
        private static int GetExceptionToken(ExceptionDispatchInfo? ex)
        { 
            if (ex is null)
                return -1;

            lock (ExceptionDbLock)
            {
                if (NextIndex >= 100)
                    NextIndex = 0;
                
                var token = NextIndex++;
                ExceptionDb[token] = ex;
                return token;
            }
        }
        
        [DebuggerHidden]
        private static ExceptionDispatchInfo? FindException(int token)
        {
            if (token == -1)
                return null;

            lock (ExceptionDbLock)
            {
                var result = ExceptionDb[token];
                if (result is null)
                    return null;
            
                if (result.SourceException is null)
                    ExceptionDb[token] = null;

                return result;
            }
        }

        #endregion
        
    }
}
