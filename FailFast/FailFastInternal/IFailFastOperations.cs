using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

public interface IFailFastOperations
{
    
    /// <summary>
    /// Tests if an object is null
    /// </summary>
    /// <param name="obj">The object you want to null check</param>
    /// <param name="caller">For logging purposes and should be handled automatically</param>
    /// <returns>true if the provided object is null</returns>
    bool Null([NotNullWhen(false)] object? obj, [CallerMemberName] string caller = "");
    
    
    /// <inheritdoc cref="IFailFastOperations.Null"/>>
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
    
    
    /// <inheritdoc cref="IFailFastOperations.True"/>>
    /// <summary>
    /// Tests if a boolean condition in your code resolved to false. 
    /// </summary>
    /// <returns>true if the provided test resolved to false</returns>
    bool NotTrue(bool? test, [CallerMemberName] string caller = "");
    
    
    /// <summary>
    /// Executes a provided method/lambda in a Try/Catch and resolves to true if an exception is thrown. 
    /// </summary>
    /// <param name="expression">A method or lambda that does something that may throw an exception</param>
    /// <param name="caller">For logging purposes and should be handled automatically</param>
    /// <returns>true if the expression provided threw an exception</returns>
    bool Throws(Action expression, [CallerMemberName] string caller = "");
    
    
    /// <inheritdoc cref="IFailFastOperations.Throws(Action, string)"/>>
    /// <param name="args">Argument(s) your expression requires for execution</param>
    bool Throws<T>(T args, Action<T> expression, [CallerMemberName] string caller = "");
}