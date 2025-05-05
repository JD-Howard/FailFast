#if !(NETCOREAPP || NETSTANDARD2_1)
namespace System.Diagnostics.CodeAnalysis
{
    // NotNullWhenAttribute does not exists in NetFramework/NetStandard2.0 projects.
    // It is a critical attribute shim that helps IDEs understand our expectations.
    internal class NotNullWhenAttribute : Attribute
    {
        /// <summary>Initializes the attribute with the specified return value condition.</summary>
        /// <param name="returnValue">
        /// The return value condition. If the method returns this value, the associated parameter will not be null.
        /// </param>
        internal NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        /// <summary>Gets the return value condition.</summary>
        internal bool ReturnValue { get; }
    }
}
#endif