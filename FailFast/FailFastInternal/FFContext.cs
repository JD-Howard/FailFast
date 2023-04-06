﻿namespace System.Diagnostics.CodeAnalysis.FailFastInternal;

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
    Log = 2,
    
    /// <summary>
    /// Performs all logic, breaking &amp; logging
    /// </summary>
    LogBreak = 3
}