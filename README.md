# FailFast
A NetStandard developer library for wrapping defensive code into something that can either be inspected or ignored depending on your needs. A great deal of thought went into avoiding development side effects being pushed to production. So, in most cases, the presence of FailFast invocations should be safe for production. However, I am considering making separate FailFast.Dev & FailFast.Prod packages that will turn all but the most primitive logic of FailFast inert. If that is something of interest, open an issue so we can discuss what needs to be in/out for Prod.

Fundamental problem FailFast is trying to solve: accessible `Debugger.Break()` calls being left in the code. If using `Debugger.Break()` is such a problem, then why not just use breakpoint conditions? Yes, those work too, but breakpoints conditions are a compounding extreme drag on the debugger. Even minimal breakpoint condition usage will be at LEAST 2x slower! So, some good defensive coding with a `Debugger.Break()` often winds up getting used somewhere. Instead of fighting it, FailFast seeks to manage the problem, create logging opportunities and hopefully lower the verbosity of your code in the process.




### Compatibility
FailFast has no dependencies, is built against NetStandard 2.0/2.1 and should generally work fine with all semi-modern Core & Framework projects. FailFast is NOT intended to be used as a dependency of other libraries. 

Note that the `[NotNullWhen]` attribute used by FailFast Null() & NotNull() methods does not exist in legacy Frameworks. There is an integrated compatibility shim for NetStandard2.0 (Framework) projects and Newer/Advanced IDEs like VS2022 & Rider both seem to use the shim appropriately.

```C#
if (FailFast.When.Null(SomeObj))
    return 0;

return SomeObj.value;
```

In that example, both Core & Framework projects will not flag SomeObj as being null while #Nullable is enabled.


### Configuration
FailFast can be used on a temporary basis without being configured, but only through use of the `FailFast.BreakWhen` route and those will literally throw errors if the DEBUG flag doesn't exist. So, they are only intended for drilling into what is directly in front of you. If that is all you need, then you'll be deleting all traces of FailFast and can entirely remove the reference from your Release build.

FailFast itself and its Configuration are intended to be singletons. You define what the configuration does by implementing the `IFailFastConfig` interface. Which only cares about 3 things: 

- bool CanDebugBreak property
  - The centralized place you can enable/disable whether `Debugger.Break()` will fire.
  - Your implementation determines whether it can manipulated at runtime. In your debug builds, you may want to allow it to be set dynamically, but your production builds may choose to swallow any effort make it True.
  - In parallel tasks, this should NOT be dynamically manipulated. Those tasks can be "All In" or "All Out" for allowing `Debugger.Break()` to fire. When fine grain control of async situations are needed, then its probably a good place to temporarily use the `FailFast.BreakWhen` route. 
- void FailFastBreak()
  - Optional logging entry point for FailFast primitive conditions that resolve to true.
- void FailFastThrows()
  - Optional exception logging entry point for the FailFast Throws() methods.

**The IFailFastConfig is specifically intended to work with the `FailFast.When` route and more often than not, that should be the only route used in your code.** 

For Production builds, it is recommended that the Logging features be enabled and CanDebugBreak always returns False.

To use your IFailFastConfig implementation:
```C#
FailFast.Initialize(yourConfig)
```

You should initialize very early in your application lifecycle. Any subsequent efforts to re-initialize will have no effect. This was a design decision for applications that use 3rd party plugins; basically it prevents 3rd party code from hijacking FailFast.  


### Usage
FailFast is great for making defensive code extremely obvious and getting some runtime/debug visibility before the program goes dead. **If your not doing some form of "bad things" detection, then don't use FailFast!**

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

In that example and assuming a config was setup to allow breaking... If any one of those FailFast calls resolves to true, then you should hit a `Debugger.Break()` and gain some insight into what went wrong without losing your debug session. You may need to do 1 Step-In/Out to get back to your code, but that should be it.


#### Throws
FailFast is also equipped to deal with arbitrary code block execution. This won't give you quite the same context your own Try/Catch would, but its pretty close and especially helpful if your leaving the FailFast code in production/logging the issues.

```C#
public int UpdateMagicNumber(int seed) 
{ 
    this.Seed = seed;
    
    FailFast.When.Throws(SeedMutator);
    
    var num = new Random.Next();    
    FailFast.When.Throws(() =>
    {
        this.Seed = this.Seed / num;
    });
    
    FailFast.When.Throws(this.Seed, SeedReducer);
    
    FailFast.When.Throws(num, (n) => 
    {
        SeedReducer(n);
    });
    
    return this.Seed;
}
```

In that contrived example and assuming a config was setup to allow breaking... If any one of those FailFast calls resolves threw an exception, then you should hit a `Debugger.Break()` and gain some insight into what went wrong without losing your debug session. It should land inside of the FailFast Try/Catch giving you the ability to inspect the exception and the same 1 Step-In/Out should get you back to your code.


----
#### Contribute
This repository is simplistic and not expected to evolve much, but definitively open to suggestions and contributions.