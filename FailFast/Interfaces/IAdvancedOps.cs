namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        internal interface IAdvancedOps : IPrimitiveOps
        {
            /// <summary>
            /// Executes a provided method/lambda in a Try/Catch and resolves to true if an exception is thrown.
            /// </summary>
            /// <param name="expression">A method or lambda that does something that may throw an exception</param>
            /// <param name="caller">For logging purposes and should be handled automatically</param>
            /// <returns>FFThrowRecovery can resolve to boolean or be used to execute OnException recovery code</returns>
            IThrowRecovery Throws(Action expression, [CallerMemberName] string caller = "");
        }
    }
}