using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis.FailFastInternal;


namespace System.Diagnostics;



public static class FailFast
{
    /// <summary>
    /// Captures the presence of a bad result regardless of whether CanDebuggerBreak=True.
    /// This could be a lot more logs than desired and you should implement required filtering.
    /// If desired, your implementation should generate the StackTrace/rewind frames as needed.
    /// </summary>
    /// <param name="caller">Method name that invoked FailFast</param>
    /// <param name="routing">The FailFast specific path syntax executed</param>
    /// <param name="context">The object or boolean that was passed to a FailFast method</param>
    public delegate void FFLogBreak(string caller, string routing, object? context);
    
    
    /// <summary>
    /// Captures exception data from the Throws() method
    /// </summary>
    /// <param name="error">The exception that was thrown in the FailFast TryCatch block</param>
    public delegate void FFLogThrow(Exception error);

    
    /// <summary>
    /// A Try/Catch wrapper that executes code passed to it from FastFail Throws() method(s)
    /// Using this does not and cannot inline the TryCatch handling. So, the user experience
    /// will not be good and will require more StepIn/Out operations to get back to the user code. 
    /// </summary>
    public delegate Exception? FFTryCatch(Action expression);
    
    
    
    
    private static readonly object ExceptionDbLock = new();
    private static readonly List<WeakReference<Exception>?> ExceptionDb = new();
    
    internal static int GetExceptionToken(Exception? ex)
    {
        if (ex is null)
            return -1;

        lock (ExceptionDbLock)
        {
            var token = ExceptionDb.Count;
            ExceptionDb.Add(new WeakReference<Exception>(ex));
            return token;
        }
    }
    
    internal static Exception? FindException(int token)
    {
        if (token == -1)
            return null;

        lock (ExceptionDbLock)
        {
            var weakPointer = ExceptionDb[token];
            if (weakPointer is null)
                return null;
            
            weakPointer.TryGetTarget(out var result);
            
            if (result is null)
                ExceptionDb[token] = null;

            return result;
        }
    }
    
    
    

    private static FFConfig? _config;
    
     /// <summary>
    /// Used internally to access the logger, but also determines whether "When" can execute Debugger.Break().
    /// Use the FailFast.Initialize() method to configure this very early in the app lifecycle.
    /// </summary>
    internal static FFConfig Config => _config ??= new();

    /// <summary>
    /// Determines whether FailFast will invoke Debugger.Break() when a condition resolve to true.
    /// </summary>
    public static bool CanDebugBreak
    {
        get => Config.CanDebugBreak;
        set => Config.CanDebugBreak = value;
    }

    
    

    /// <summary>
    /// Used to configure FailFast for logging, centralized Debugger.Break() authorization &amp; to pull the TryCatch handling into your code.
    /// The canBreak argument affects the initial state of FailFast.CanDebuggerBreak and determines if it can be dynamically manipulated.
    /// However, FailFast.CanDebuggerBreak should NEVER be manipulated within parallel tasks.
    /// For async work, you need to be all in, all out, or using "BreakWhen" &amp; convert it to "When" for Releases builds.
    /// </summary>
    /// <param name="canBreak">CanDebugBreak default value &amp; dynamic manipulation behavior</param>
    /// <param name="inlineTryCatch"> 
    /// This puts the context of the Try/Catch in your code so FF can properly swallow/log errors and step-in/out gets back to your code faster.
    /// <code>(exp) => try { exp(); return null; } catch (Exception e) { return e; }</code>
    /// </param>
    /// <example>inlineTryCatch Lambda Example
    /// <code>(exp) => try { exp(); return null; } catch (Exception e) { return e; }</code>
    /// </example>
    /// <exception cref="InvalidOperationException">
    /// Throws if executed more than once and is intended to prevent FailFast hijacking. However, this could also
    /// happen if you are initializing FailFast too late in the app lifecycle and the defaults were already applied.
    /// </exception>
    public static void Initialize(FFBreakOption canBreak, FFTryCatch inlineTryCatch)
    {
        if (_config is not null)
            throw new InvalidOperationException("FailFast cannot be initialized more than once. Can you initialize it sooner?");

        _config = new FFConfig(canBreak, null, null, inlineTryCatch);
    }

    
    /// <inheritdoc cref="FailFast.Initialize(FFBreakOption, FFTryCatch)"/>
    /// <param name="logBreakDelegate">optional log entry point for non-Throws() methods that resolve to true</param>
    /// <param name="logThrowDelegate">optional log entry point for Throws() if it produced an exception</param>
    public static void Initialize(FFBreakOption canBreak, FFTryCatch inlineTryCatch, FFLogBreak logBreakDelegate, FFLogThrow logThrowDelegate)
    {
        if (_config is not null)
            throw new InvalidOperationException("FailFast cannot be initialized more than once. Can you initialize it sooner?");

        _config = new FFConfig(canBreak, logBreakDelegate, logThrowDelegate, inlineTryCatch);
    }

    
    

    /// <summary>
    /// The Debugger.Break() C2 permissions are delegated to the FailFast Configuration.
    /// All operations resolving to True will attempt to generate logs.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if FailFast.Initialize() was not properly pre-configured</exception>
    public static IFFAdvancedOperations When
    {
        get
        {
            if (Config.IsDefault)
                throw new InvalidOperationException("FailFast.When statements depend on FailFast.Initialize() prerequisites");
            
            return new FFOperations(CanDebugBreak && Debugger.IsAttached ? FFContext.LogBreak : FFContext.Log);
        }
    }


    /// <summary>
    /// Does not require configuration, but never produces logs and will not provide the Throws() routing.
    /// This is NOT for production builds &amp; always forces a Debugger.Break() if the operation resolves to True + debugger is attached.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if called and the DEBUG flag is not defined</exception>
    public static IFFPrimitiveOperations BreakWhen
    {
        get
        {
            var hasDebugDirective = false;
            UpdateDebugStatus(ref hasDebugDirective);

            if (!hasDebugDirective)
                throw new InvalidOperationException("FailFast.BreakWhen statements should only exist in DEBUG builds");
                
            
            // Do not break without debugger. The Debugger.Break() has an effect even without an attached debugger.
            // This defensive strategy is good for scenarios where a debug build is being used as an internal demo.
            return new FFOperations(Debugger.IsAttached ? FFContext.Break : FFContext.None);
        }
    }
    

    /// <summary>
    /// This should only ever executes if the DEBUG environment variable is defined.
    /// Kind of a round about way to help consumers avoid volatile side effects.
    /// </summary>
    [Conditional("DEBUG")]
    private static void UpdateDebugStatus(ref bool isDebugMode) => isDebugMode = true;
}