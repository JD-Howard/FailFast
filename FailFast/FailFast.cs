using System.Diagnostics.CodeAnalysis.FailFastInternal;


namespace System.Diagnostics;


public class FailFast
{
    private static FailFast? _instance;
    private readonly IFailFastConfig? _config = null;
    
    /// <summary>
    /// Used internally to access the logger, but also determines whether "When" can execute Debugger.Break().
    /// Your IFailFastConfig implementation should be provided to Initialize() very early in the app lifecycle.
    /// </summary>
    public static IFailFastConfig? Config => _instance?._config;
    
    
    /// <summary>
    /// Uses private constructors since this is intended to be a Singleton utility.
    /// </summary>
    private FailFast(IFailFastConfig config) => _config = config;
   
    
    /// <summary>
    /// Used to configure the FF singleton for logging &amp; centralizes whether the debugger can hit dynamic breakpoints.
    /// The CanDebuggerBreak property on the config can be dynamically manipulated to control Debugger.Break() execution.
    /// However, it should never be manipulated within parallel tasks.
    /// For async work, you need to be all in, all out, or using "BreakWhen" &amp; convert it to "When" for Releases builds.
    /// If your app loads 3rd party plugins, then you should either use compiler directives to remove FailFast code or
    /// at least doing an early initialize with an IFailFastConfig implementation where CanDebuggerBreak is always false. 
    /// </summary>
    public static void Initialize(IFailFastConfig config)
        // The ??= will allow it to be configured only once. This will help prevent hijacking by 3rd party plugins.
        => _instance ??= new (config); 
    
    
    /// <summary>
    /// The Debugger.Break() C2 permissions are delegated to the provided FailFast Configuration.
    /// All logical operations resolving to True will attempt to generate logs.
    /// Note: you do not need to configure FF, but this path cannot fire Debugger.Break() without a configuration.
    /// </summary>
    public static IFailFastOperations When 
        => new FailFastOperations(Config?.GetCanDebugBreak() == true ? FailFastContext.ConfigBreak : FailFastContext.NoBreak);


    
    /// <summary>
    /// Not for production builds and always forces a Debugger.Break() if the operation resolves to True.
    /// However, this also bypasses the FailFast Configuration and will never generate any logs.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if called and the DEBUG flag is not defined</exception>
    public static IFailFastOperations BreakWhen
    {
        get
        {
            var hasDebugDirective = false;
            UpdateDebugStatus(ref hasDebugDirective);
            
            if (hasDebugDirective || Debugger.IsAttached)
                return new FailFastOperations(FailFastContext.ExplicitBreak);
            
            throw new InvalidOperationException("FailFast.BreakWhen statements should only exist in DEBUG builds");
        }
    }

    /// <summary>
    /// This should only ever executes if the DEBUG environment variable is defined.
    /// Kind of a round about way to help consumers avoid volatile side effects.
    /// </summary>
    [Conditional("DEBUG")]
    private static void UpdateDebugStatus(ref bool isDebugMode) => isDebugMode = true;
}