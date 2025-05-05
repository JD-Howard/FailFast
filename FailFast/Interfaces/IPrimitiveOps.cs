namespace System.Diagnostics
{
    internal static partial class FailFast
    {
        internal interface IPrimitiveOps
        {
            /// <summary>
            /// Tests if an object is null
            /// </summary>
            /// <param name="obj">The object you want to null check</param>
            /// <param name="caller">Only for logging purposes and should be handled automatically</param>
            /// <returns>true if the provided object was null</returns>
            bool Null([NotNullWhen(false)] object? obj, [CallerMemberName] string caller = "");


            /// <inheritdoc cref="IPrimitiveOps.Null"/>>
            /// <summary>
            /// Tests if an object is not null
            /// </summary>
            /// <returns>true if the provided object is not null</returns>
            bool NotNull([NotNullWhen(true)] object? obj, [CallerMemberName] string caller = "");


            /// <summary>
            /// Tests if a boolean condition in your code resolved to true. 
            /// </summary>
            /// <param name="test">The result of your boolean operation</param>
            /// <param name="caller">For logging purposes and should be handled automatically</param>
            /// <returns>true if the provided test resolved to true</returns>
            bool True(bool? test, [CallerMemberName] string caller = "");


            /// <inheritdoc cref="IPrimitiveOps.True"/>>
            /// <summary>
            /// Tests if a boolean condition in your code resolves to a falsy value.
            /// Use the Null() or NotNull() methods if null has specific meaning in your code base. 
            /// </summary>
            /// <returns>true if the provided test does not resolve to true</returns>
            bool NotTrue(bool? test, [CallerMemberName] string caller = "");
        }
    }
}