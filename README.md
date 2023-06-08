# MalbolgeSharp

A basic lightweight implementation of Malbolge in C#.

This is more or less a re-implementation of [the original Malbolge interpreter](http://esoteric.sange.fi/orphaned/malbolge/malbolge.c). Malbolge is intentionally obtuse and nigh impossible to work with. During my research I have seen several variations. I try to support this variation by incorporating different `MalbolgeFlavor` options, but usually it's too difficult to find what aspect was changed or done incorrectly. Because of this, you may find that some Malbolge programs on the internet do not work with this implementation. I have minimized this to the best of my abilities.

This implementation seeks to be as fast and lightweight as possible, and uses minimal allocations to keep pressure off the garbage collector. It is not zero allocation, as large arrays are more performant for these applications, but they are recycled via an `ArrayPool`.
