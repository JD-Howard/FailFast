# Integration Test Expectations
FailFast has a lot of behaviors that cause debuggers to break or throws protective compilation errors in a given context and thus is nearly impossible to fully unit test. This document serves as the manual tests that should be performed before creating a new Release/Nuget package.

For full coverage, this project has various compiler directive driven configurations and each of these test sections will be based on a specific configuration.

## Configuration: Production
This configuration does not have the DEBUG symbol and the test project compiler directives have left a PRODUCTION block containing a `BreakWhen.True()` statement. This should cause a compilation error. 

## Configuration: Release

## Configuration: Debug