# FailFast
A NetStandard developer library for wrapping defensive code into something that can either be inspected, logged or ignored depending on your need/environment. A great deal of thought went into avoiding development side effects being pushed to production. So, in most cases, the presence of FailFast invocations should be safe for production.


### What does FailFast Solve?
Make defensive code more succinct and prevent accessible `Debugger.Break()` calls being left in production code. If using `Debugger.Break()` is such a problem, then why not just use breakpoint conditions? Yes, those work too, but breakpoints conditions are a compounding extreme drag on the debugger. Even minimal breakpoint condition usage will be a **minimum** of 2x slower! So, some good defensive coding with a `Debugger.Break()` often winds up getting used somewhere. Instead of fighting it, FailFast seeks to manage the problem, create logging opportunities and potentially lower the verbosity of defensive code in the process.



### Compatibility
FailFast will never have any non-core dependencies, is built against NetStandard 2.0/2.1 and should generally work fine with all semi-modern Core/Framework projects. FailFast is **NOT** intended to be used as a dependency of other libraries. It could be used on strictly internal libraries where it is **known** to have been configured early in the main applications lifecycle, but that is the only exception.

Note that the `[NotNullWhen]` attribute used by FailFast Null() & NotNull() methods does not exist in legacy Frameworks. There is a compatibility shim for NetStandard2.0 (Framework) projects and Newer/Advanced IDEs like VS2022 & Rider both seem to use the shim appropriately.

```C#
if (FailFast.When.Null(SomeObj))
    return 0;

return SomeObj.Value;
```

In that example, both Core & Framework projects will not flag SomeObj as being null while #Nullable is enabled.


### Configuration
FailFast can be used on a temporary basis without being configured, but only through use of the `FailFast.BreakWhen` route and those calls will throw exceptions if the DEBUG flag doesn't exist. So, they are only intended for drilling into what is directly in front of you. If that is all you need, then you'll be deleting all traces of FailFast and can entirely remove the reference from your Release build anyway.

FailFast itself is a static class and can only be configured once. This prevents FailFast from being hijacked in applications that allow 3rd party plugins. To configure FailFast, just execute the `Initialize()` method, provide it a breaking behavior and 1-3 methods it should use for enhanced functionality. 

- **FFBreakOption** determines the starting authorization level for using `Debugger.Break()` and whether it can be dynamically changed using the `FailFast.GetCanDebugBreak` Property.
  - InitFalse: initializes to False, but can be changed.
  - InitTrue: initializes to True, but can be changed.
  - StaticFalse: initializes to False and **cannot** be changed.
    - This is the default value if `Initialize()` is not executed.
    - This should be the default value for all Production builds.
  - StaticTrue: initializes to True and **cannot** be changed.
- **delegate Exception? (Action expression)**
  - This is **required** for using the `FailFast.When` routing and more specifically the `Throws()` method.
  - By providing a Try/Catch block from your code base that FailFast can "use", it helps your attached debugger respect the `[DebuggerHidden]` attributes during a FailFast call to `Debugger.Break()`. Which basically allows you to do a single Step-In/Out to get back to your code.
  - Without this, a FailFast Try/Catch would not be able to encapsulate the exceptions your code produces. The FF Try/Catch will prevent a fatal error and let you potentially handle it, but cannot prevent/manage them.
  - Lambda Example:
    - `(exp) => try { exp(); return null; } catch (Exception e) { return e; }`
- **delegate void (Exception error)**
  - Optional log point for the `FailFast.When` routing, but will still generate logs while CanDebugBreak = False. Every log system should already have a method that matches this signature.
  - Specifically associated with the `FailFast.When.Throws()` method. 
- **delegate void (string caller, string routing, object? context)**
  - Optional log point for the `FailFast.When` routing, but will still generate logs while CanDebugBreak = False.
  - Specifically for the Null, NotNull, True & NotTrue methods.
  - Argument Definitions:
    - caller: The method name (your code) that called something in FailFast.
    - routing: What was called in FailFast, IE "FailFast.When.NotNull"
    - context: What FF was provided. So it could be a boolean, null or an object.
  - **Note:** this was **not** an exception, this was a predicted "bad scenario" and may have subsequent compensating code. So, if you want a stacktrace, you'll have to pull it in your implementation and roll it back as desired.

**You should initialize very early in your application lifecycle. Any subsequent efforts to re-initialize will throw an exception.**

#### Dynamic CanDebugBreak Manipulation
FailFast was designed for DEBUG build runtime manipulation of the `FailFast.CanDebugBreak` property and is highly recommended over BreakPoint conditions; mostly because of performance... However, this variable is a centralized place your entire application reads and when equal to true, then any FailFast condition resolving to true will cause a `Debugger.Break()`. With that said, asynchronous code either needs to be "All-In", "All-Out" or decoupled by using the `FailFast.BreakWhen` on a temporary basis.

Other than the rule above, it is probably best practice (even in DEBUG builds) to initialize FailFast to `InitFalse`, code everything using `FailFast.When` and explicitly turn on/off the areas you want to actively capture. If your production code is initializing with `StaticFalse`, then even forgetting to remove those explicit `CanDebugBreak` sets will not affect your production build; they will be ignored...

**If your not using the logging features**, then I **highly recommended** using `InitTrue` as your default DEBUG initialize value. At least do this occasionally, or else you may have no clue what FailFast is masking.


### Usage
FailFast is great for making defensive code extremely obvious and getting some runtime/debug visibility before the debug session dies. **If your not doing some form of "bad things" detection, then don't use FailFast!**

FailFast implements the following primitive tests:
- Null(object? obj)
- NotNull(object? obj)
- True(bool? test)
- NotTrue(bool? test)

#### Primitive Example
```C#
public void SomeMethod<T>(T obj) where T : ISomeCollection 
{
    if (FailFast.When.Null(obj) || FailFast.When.NotNull(obj.Next))
        return;
        
    FailFast.When.True(obj.Length == 0)
    FailFast.When.NotTrue(obj.Length <= 100);
    
    obj.Add(obj.Last * 2);
}
```

The above example would throw an error if the `FailFast.Initialize()` method was never executed, but every `When` statement could be replaced with `BreakWhen` and be perfectly valid in an uninitialized state. 

Assuming a `CanDebugBreak` state of `true`, if any one of the above FailFast calls resolves to true, then you should hit a `Debugger.Break()` and gain some insight into what went wrong without losing your debug session. You may need to do 1 Step-In/Out to get back to your code, but that should be it.


#### Throws Example
FailFast is also equipped to deal with arbitrary code block execution. This won't give you quite the same context your own Try/Catch would, but its pretty close, provides succinct failure recovery and is especially helpful if your leaving the FailFast code in production/logging the exceptions.

```C#
public int UpdateMagicNumber(int seed) 
{ 
    // using lambda, that captures variables, has a specific 
    // recovery stategy and re-throws if not an expected problem.
    FailFast.When.Throws(() => 
    {
        this.Seed = this.Seed / seed; 
    }).OnError(ex => 
    {
        if (ex is not DivideByZeroException)
            throw new UnhandledExceptionEventArgs(ex);
        
        this.Seed = seed;
    });
     
    // using a concrete method group
    FailFast.When.Throws(SeedMutator);
    
    var num = new Random.Next();
    
    // using method group that takes an argument (avoids variable capture)
    FailFast.When.Throws(SeedReducer, this.Seed);
    
    // using lambda that takes an argument (avoids variable capture)
    FailFast.When.Throws(n => 
    {
        SeedReducer(n);
    }, num);
    
    return this.Seed;
}
```
The above example would throw an error if the `FailFast.Initialize()` method was never executed.

In that contrived example and assuming `CanDebugBreak == true`... If any one of those FailFast `Throws()` calls threw an exception, then you should hit a `Debugger.Break()` and get some insight into what went wrong without losing your debug session. It should land inside of the FailFast Try/Catch giving you the ability to inspect the exception and the same 1 Step-In/Out should get you back to your code.

### Note on Capturing
Capturing is bad... However, its not that bad and even native functionality (like async) will do very similar things. If your doing all kinds of stuff to avoid it, then you need a good reason. In most cases, it would largely be considered an unnecessary micro-optimization. I've provided method overloads that take (4 max) arguments to help you avoid some of the capturing, but I think it makes the code less clear and I wouldn't recommend using them except where truly necessary.

----
#### Contribute
This repository is simplistic and not expected to evolve much, but definitively open to suggestions and contributions.