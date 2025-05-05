// ============================================================================
// Author: Joshua Howard
// Project: FailFast
// Version: 2.0.0.0
// License: MIT License
// Source: https://github.com/JD-Howard/FailFast
// 
// Description: A NetStandard developer library for wrapping defensive code
//              into something that can either be inspected, logged or ignored
//              depending on your need/environment. Effort went into avoiding
//              development side effects being pushed to production. In most
//              cases, the FailFast invocations should be okay in production.
//
// TIPS:
//   1. You must have your CSPROJ configured for LangVersion 8 or higher:
//      <LangVersion>8</LangVersion>
//   2. Async is fine, but avoid FailFast calls in overly parallel tasks.
//   3. Highly recommend changing the namespace to match your project root.
//   4. If your project references another project using FailFast and you are
//      not in a solution containing its code, then you should reference a
//      production build DLL that has FailFast turned off.
//   5. FailFast could be functional as a separate compiled dependency, but it
//      is absolutely critical the consumer provides the FFTryCatch delegate.
// 
// Copyright (c) 2025 Joshua Howard
// ============================================================================

using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace System.Diagnostics
{
    internal static class FailFast
    {

        #region Configure.cs

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
        
        

        #endregion // Configure.cs


        #region FailFast.cs



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
        

        #endregion // FailFast.cs


        #region Operations.cs

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
        

        #endregion // Operations.cs


        #region ThrowRecovery.cs

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
        
        

        #endregion // ThrowRecovery.cs


        #region FFBreakOption.cs

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


        #endregion // FFBreakOption.cs


        #region FFContext.cs


        [Flags]
        internal enum FFContext
        {
            /// <summary>
            /// Performs logic only; no-break no-log
            /// </summary>
            None = 0,

            /// <summary>
            /// Debugger.Break() is Authorized
            /// </summary>
            Break = 1,

            /// <summary>
            /// The 2 logging functions will be used if available
            /// </summary>
            Log = 2
        }


        #endregion // FFContext.cs


        #region IPrimitiveOps.cs

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

        #endregion // IPrimitiveOps.cs


        #region IAdvancedOps.cs

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

        #endregion // IAdvancedOps.cs


        #region IThrowRecovery.cs

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

        #endregion // IThrowRecovery.cs

    }
}

#region IdeShim.cs

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

#endregion // IdeShim.cs

