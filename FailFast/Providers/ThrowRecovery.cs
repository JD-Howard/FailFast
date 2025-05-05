namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        internal readonly struct ThrowRecovery : IThrowRecovery
        {
            private readonly bool _result;
            private readonly int _exceptionToken;
            private readonly bool _handled;
           
            
            /// <inheritdoc cref="IThrowRecovery.Result"/>
            public bool Result => _result;

            
            /// <summary>
            /// Completes the FailFast.When.Throws() chain for handling thrown exceptions. 
            /// </summary>
            /// <param name="result">Result of the original Throws() method</param>
            /// <param name="errToken">If Throws() generated an exception, it can be retrieve with TryGetExceptionSource()</param>
            /// <param name="handled">Set to true if an On&lt;T&gt; handler matched the type of exception</param>
            [DebuggerHidden]
            internal ThrowRecovery(bool result, int errToken, bool handled)
            {
                _handled = handled;
                _result = result;
                _exceptionToken = errToken;
            }

            
            /// <inheritdoc cref="IThrowRecovery.OnException(Action&lt;ExceptionDispatchInfo&gt;)"/>
            [DebuggerHidden]
            public bool OnException(Action<ExceptionDispatchInfo> expression)
            {
                if (_result == false)
                    return false;
                
                if (_handled == true)
                    return true;
                
                var exInfo = FindException(_exceptionToken);
                if (exInfo != null)
                    expression(exInfo);

                return true;
            }


            /// <inheritdoc cref="IThrowRecovery.On&lt;Exception&gt;"/>
            [DebuggerHidden]
            public IThrowRecovery On<T>(Action<ExceptionDispatchInfo> expression) where T : Exception
            {
                // TODO: Currently, all Throws() that cause an exception will try logging the exception.
                //       However, "handled" versions, especially this On<T> has a very real desire to 
                //       ignore logging something that was expected. This will probably need a new enum.

                
                

                if (_handled)
                    return this; // short circuit if a previous On<T>() already handled the chain
                
                var exInfo = FindException(_exceptionToken);
                if (exInfo?.SourceException is T)
                {
                    expression(exInfo);
                    return new ThrowRecovery(_result, _exceptionToken, true);
                }

                return this;
            }
        }
        
        
    }
}