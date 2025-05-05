# FailFast
A NetStandard developer library for wrapping defensive code into something that can either be inspected, logged or ignored depending on your need/environment. A great deal of thought went into avoiding development side effects being pushed to production. In most cases, the presence of FailFast invocations should be safe for production.

### v2.0.0 Important Note 
FailFast was converted to a single CS file output release because the `[DebuggerHidden]` flag was not being respected by a compiled Try/Catch. There was a clunky workaround of consumers providing their own Try/Catch delegate, but a mandatory configuration element was not acceptable constraint.

### What does FailFast Solve?
Make defensive code more succinct and prevent accessible `Debugger.Break()` calls being left in production code. If using `Debugger.Break()` is such a problem, then why not just use IDE breakpoint conditions? Yes, those work too, but breakpoints conditions are a compounding extreme drag on the debugger. Even minimal breakpoint condition usage will be a **minimum** of 2x slower! So, some good defensive coding with a `Debugger.Break()` often winds up getting used somewhere. Instead of fighting it, FailFast seeks to manage the problem, create logging opportunities and potentially lower the verbosity of defensive code in the process.



### Compatibility
FailFast will never have any non-core dependencies, is built to support NetStandard 2.0 and should generally work fine with all semi-modern Core/Framework projects. FailFast is **NOT** intended to be used as a dependency of other libraries. It should always be included in the project that is using it. Also highly recommend the FailFast class namespace is updated to match your root project namespace.

Note that the `[NotNullWhen]` attribute used by FailFast Null() & NotNull() methods does not exist in legacy Frameworks. There is a compatibility shim for NetStandard2.0 (Framework) projects and Newer/Advanced IDEs like VS2022 & Rider both seem to use the shim appropriately. Directives put it out of scope automatically, but if your project has its own shim for this attribute, then delete it from the bottom of the `FailFast.cs` file.

```C#
if (FailFast.When.Null(SomeObj))
    return 0;

return SomeObj.Value;
```

In that example, your IDE in both Core & Framework projects will not flag SomeObj as potentially being null while #Nullable is enabled.


### Configuration
FailFast can be used on a temporary basis using the `FailFast.BreakWhen` route and those calls will throw exceptions if the DEBUG flag doesn't exist. The BreakWhen path is only intended for drilling into whatever is directly in front of you. If that is all you need, then you will be actively debugging and then deleting the FailFast calls. It was configured to throw build and runtime errors if not debugging because you do NOT want an explicit break call in your Release builds; these still produce side effects without a debugger attached.

FailFast itself is a static internal class and should be isolated from standard 3rd party consumer manipulation except during reflection. This prevents FailFast from being hijacked in applications that allow 3rd party plugins. To configure FailFast, go set the various `FailFast.Configure` properties. All of these have inline documentation. 


- **Enum** `FFBreakOption` determines the starting authorization level for using `Debugger.Break()` and whether it can be dynamically changed at runtime using the `FailFast.Configure.BreakBehavior` Property.
  - InitFalse: initializes to False, but can be changed at runtime.
    - This is the default value if the DEBUG directive is defined.
  - InitTrue: initializes to True, but can be changed at runtime.
  - StaticFalse: initializes to False and **cannot** be changed if set.
    - This is the default value if the DEBUG directive is NOT defined.
  - StaticTrue: initializes to True and **cannot** be changed if set.
    - Occasionally using this in a debug build can help you discover what FailFast might be masking.
- **Delegate** `FFLogBreak(string caller, string routing, object? context)`
  - Optional and used by the `FailFast.Configure.BreakLogHandler` Property. This provides logging opportunities for any build configuration type and doesn't really care about `BreakBehavior` state or if a debugger is attached. If you don't want to generate logs in a debug build, then use compiler directives to only configure it on non-debug builds.
  - Argument Breakdown:
    - *caller:* The method that that called FailFast.
    - *routing:* FailFast specific execution chain.
    - *context:* The object or boolean passed to a FailFast method.
- **Delegate** `FFLogThrow(string caller, ExceptionDispatchInfo error)`
  - Optional and used by the `FailFast.Configure.ThrowsLogHandler` Property. This provides logging opportunities for any build configuration type whenever `FailFast.When.Throws()` generates an exception and doesn't really care about `BreakBehavior` state or if a debugger is attached. If you don't want to generate logs in a debug build, then use compiler directives to only configure it on non-debug builds.
  - Argument Breakdown:
    - *caller:* The method that that called FailFast.
    - *error:* A fully captured exception that can be logged or rethrown at a later time while maintaining the original context.
      - https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions#capture-exceptions-to-rethrow-later

**Note: You should configure/initialize FailFast very early in your application lifecycle.** Also note that FailFast being configured for NetStandard2.0 means it is unaware of the standard `ILogger` interface, and this is why you need to handle that on your end by providing delegates.


### Dynamic BreakBehavior Manipulation
FailFast was designed for DEBUG build runtime manipulation of the `FailFast.Configure.BreakBehavior` property and is highly recommended over BreakPoint conditions; mostly because of performance... However, this property is shared across your entire application, and when set to a Truthy state, then any FailFast condition resolving to true will cause a `Debugger.Break()`. With that said, highly asynchronous code either needs to be "All-In", "All-Out" or decoupled by using the `FailFast.BreakWhen` on a temporary basis. 

Other than the rule above, it is probably best practice (even in DEBUG builds) to initialize FailFast to `InitFalse`, code everything using `FailFast.When` and explicitly turn it on/off in the areas you want to actively inspect. If your production code is initializing with `StaticFalse`, then even forgetting to remove those explicit `BreakBehavior` manipulations will not affect your production build; they will be ignored because StaticFalse and StaticTrue cannot be altered at runtime.



### Usage
FailFast is great for making defensive code extremely obvious and getting some runtime/debug visibility before the debug session dies. **If your not doing some form of "bad things" detection, then don't use FailFast!**

FailFast implements the following primitive tests:
- Null(object? obj)
- NotNull(object? obj)
- True(bool? test)
- NotTrue(bool? test)

#### IPrimitiveOps Example
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

The above is a contrived example, but does show all of the basic primitives. If that example was using the default configuration, all of those `FailFalse.When` would not cause a `Debugger.Break()`. If you did want them to break, then you actually need to configure the `BreakBehavior` property with a true status type. Also note, all of these `When` statement could be replaced with `BreakWhen` and be perfectly valid without configuring the `BreakBehavior` property state. 

Assuming a truthy `BreakBehavior` state, if any one of the above FailFast calls resolves to true, then you should hit a `Debugger.Break()` and you would gain some insight into what went wrong without losing your debug session. Once the `Break()` runs, it should only require 1 Step-In/Out to get back to your code.


#### IAdvancedOps (Throws) Example
The IAdvancedOps adds the `Throws()` functionality to the IPrimitiveOps interface. This makes FailFast capable of Try/Catch capturing a code block while continuing to wrap consistent Logging/Breaking behaviors, and creating opportunities for exception recovery paths. This won't give you quite the same context your own Try/Catch would, but its pretty close if you dig into the `ExceptionDispatchInfo.SourceException` property, it provides succinct failure recovery, is especially helpful if your leaving the FailFast code in production, and desire some standardized logging of exceptions.

```C#
public int UpdateMagicNumber(int seed) 
{ 
    // using lambda, that captures variables, has a specific 
    // recovery stategy and re-throws if not an expected problem.
    FailFast.When.Throws(() => 
    {
        this.Seed = this.Seed / seed; 
    }).On<DivideByZeroException>(ex => 
    {
        this.Seed = seed;
    }).OnException(ex => 
    {
        ex.Throw();
    });
     
    // using a void method group
    FailFast.When.Throws(SeedMutator);
    
    var num = new Random.Next();
    return this.Seed;
}
```

In that example and assuming `BreakBehavior` is init or static true... If any one of those FailFast `Throws()` calls threw an exception, then you should hit a `Debugger.Break()` and get some insight into what went wrong without losing your debug session. It should land inside of the FailFast `Debugger.Break()` giving you the ability to inspect the `ex.SourceException` and 1 Step-In/Out should get you back to your code.

### Note on Capturing
Capturing is bad... However, its not that bad and even native functionality (like async) will do very similar things. While `FailFast.When.Throws()` does regularly capture variables, in most cases, avoiding it would be an unnecessary micro-optimization.

----

#### Contribute
This repository is simplistic and not expected to evolve much, but definitively open to suggestions and contributions.