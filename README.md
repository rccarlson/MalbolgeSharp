# MalbolgeSharp

A basic implementation of Malbolge in C#.

Malbolge is designed to be as impenetrable as possible, so there are many quirks to the language that must be accounted for. When using the library, you will note that a `MalbolgeFlavor` is required. This is because the Malbolge specs are NOT the same as the Malbolge program. According to the author, Ben Olmstead, this is intentional and he enjoys our suffering (some interpretive liberties may have been taken the latter half of that). Therefore, you must specify whether the program is to follow the rules of the spec or the Malbolge compiler implementation.

This implementation is based on the [Wikipedia](https://en.wikipedia.org/wiki/Malbolge) and [Elsolang](https://esolangs.org/wiki/Malbolge) articles on Malbolge, rather than the [original specs](http://www.lscheffer.com/malbolge_spec.html) simply because it was easier that way. The original specs are quite painful to pick apart and honestly, I couldn't be bothered.

