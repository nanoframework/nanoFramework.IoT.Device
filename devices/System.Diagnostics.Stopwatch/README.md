# System.Diagnostics.Stopwatch and DelayHelper

`Stopwatch` implementation for .NET nanoFramework.

Includes as well DelayHelper for precise Milliseconds and Microseconds delays. 

> Note: this may not be precise enough in managed code. Depending on the platform, you may have to adjust the delays and also look at `Thread.SpinWait` function.
