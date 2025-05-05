namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        internal enum FFBreakOption
        {
            /// <summary>
            /// Sets the CanDebugBreak default value to FALSE and allows dynamic manipulation at runtime.
            /// This is the default value if FailFast is not explicitly configured.
            /// </summary>
            InitFalse = 0,

            /// <summary>
            /// Sets the CanDebugBreak default value to TRUE and allows dynamic manipulation at runtime.
            /// </summary>
            InitTrue = 1,

            /// <summary>
            /// Sets the CanDebugBreak default value to FALSE and prevents any attempt to manipulate it at runtime.
            /// This should be the value for any of your deployment/release builds.
            /// </summary>
            StaticFalse = 10,

            /// <summary>
            /// Sets the CanDebugBreak default value to TRUE and prevents any attempt to manipulate it at runtime.
            /// </summary>
            StaticTrue = 11
        }

    }
}