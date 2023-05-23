# MalbolgeSharp

A basic implementation of Malbolge in C#.

Malbolge is designed to be as impenetrable as possible, so there are many quirks to the language that must be accounted for. The primary shortcoming of this implementation is that it intentionally changes part of the instruction set to match a [particular use case](http://www2.latech.edu/~acm/helloworld/malbolge.html). This is obviously a huge limitation and represents a major need for improvement. At some point, I will hopefully come back and diagnose the issue.

This implementation is based on the [Wikipedia article on Malbolge](https://en.wikipedia.org/wiki/Malbolge), rather than the [original specs](http://www.lscheffer.com/malbolge_spec.html) simply because it was easier that way. The original specs are quite painful to pick apart and honestly, I couldn't be bothered.