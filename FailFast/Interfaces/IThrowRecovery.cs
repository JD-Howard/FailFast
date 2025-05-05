namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        internal interface IThrowRecovery
        {
            /// <summary>
            /// True if the expression provided to FailFast.When.Throws() actually threw an exception.
            /// </summary>
            bool Result { get; }


            /// <summary>
            /// Use this to execute code that should recover from a FailFast.When.Throws() exception.
            /// This can be combined with leading 'On&lt;T&gt;' specific exceptions catches. However, this
            /// does not execute the 'Action' unless Throws() threw an exception. Note that the OnException
            /// method does not have a Try/Catch and the provided expression should either be bullet proof
            /// or otherwise be intending to throw. Use ExceptionDispatchInfo.Throw() to rethrow the exception.
            /// </summary>
            /// <param name="expression">Method that receives a ExceptionDispatchInfo context and performs desired recovery actions</param>
            /// <returns>True/False (passthrough) from the originating Throws() test expression</returns>
            bool OnException(Action<ExceptionDispatchInfo> expression);


            /// <summary>
            /// Use the 'On&lt;T&gt;' method to handle very specific exceptions. This is a fluent interface so you can create custom
            /// recovery logic for a variety of different possible exceptions; which is better practice than only using the OnException
            /// catch-all. The return type is a notable difference, but you can access the 'Result' property if you need a bool that
            /// specifies if an exception was created during the Throws() execution. 
            /// </summary>
            /// <param name="expression">Method that receives a ExceptionDispatchInfo context and performs desired recovery actions</param>
            /// <typeparam name="T">Must derive from Exception</typeparam>
            /// <returns>IThrowRecovery, for chaining multiple 'On&lt;T&gt;' handlers</returns>
            IThrowRecovery On<T>(Action<ExceptionDispatchInfo> expression) where T : Exception;
        }
    }
}