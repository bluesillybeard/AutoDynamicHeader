# Automatic Dynamic Header

Libraries like [XDL](https://github.com/ColleagueRiley/XDL) exist, to load certain libraries at runtime instead of compiling against it. But, making one is tedious and painful.

Using this program is simple, and self explanitory: take a .h file of a regular library, and convert it into a new .h file that can load the .so/.dll/.dylib/whatever else at runtime after calling a function. The new header replaces the function declarations of the old one, so it must be included *after* the source one, like so:

```C
// The orignal header contains original type and function declarations
#include <original/header.h>
// The new header redeclares all of the functions to be loaded dynamically
#include <dynamic/header.h>
```

## Installing the program
This is made using .NET AOT, so it compiles down to a single binary. Add the binary to your PATH, and you're done! build from source using .NET. This only uses the standard library, so it should just work.

## Usage
autodynamicheader [source] [destination] [name of load function]

The source and destination should be clear. The name of the load function is just what the function to load all the functions is called. Here is an example if "xload" is entered:
```C
void xload(void *(*load_fn)(const char* name)) {
    ...
}
```
The function takes a function pointer to a function that will load the function given the function's name.

## Notes
This utility DOES NOT properly support C++ headers. It simply looks for things that look like function declarations, and goes based off of that. Also, it is intended to do the tedious part of writing a loader header, some manual post-processing of the resulting artifact may be required.

The resulting .h file currently may or may not work in C++.

