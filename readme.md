# Automatic Dynamic Header

Libraries like [XDL](https://github.com/ColleagueRiley/XDL) exist, to load certain libraries at runtime instead of compiling against it. But, making one is tedious and painful. In fact, I am surprised nobody else has made this before. So, here we are!

Using this program is simple, and self explanitory: take a .h file of a regular library, and convert it into a new .h file that can load the .so/.dll/.dylib/whatever else at runtime after calling a function. The new header replaces the function declarations of the old one, so it must be included *after* the source one, like so:

```C
// The orignal header contains original type and function declarations
#include <original/header.h>
// The new header redeclares all of the functions to be loaded dynamically
#include <dynamic/header.h>
```

## NOTICE
Right now, all this does is print all of the functions it finds. Not ready for use!

## Installing the program
This is made using .NET AOT, so it compiles down to a single binary. Add the binary to your PATH, and you're done! build from source using .NET. This only uses the standard library, so it should just work.

## Usage
autodynamicheader [source] [destination] [name of load function]

The source and destination should be clear. The name of the load function is just what the function to load all the functions is called. Here is an example if "xload" is entered:
```C
/// @brief xload takes a pointer to a function that takes the name of the function to load and returns a function pointer to that function, and uses that to dynamically load all of the functions. You know, I think writing this out makes me realize how bad I am at explaining things.
void xload(void *(*load_fn)(const char* name)) {
    ...
}
```

This utility DOES NOT properly support C++ headers. It simply looks for things that look like function declarations, and goes based off of that.

## Use cases

This is an admittedly very niche program. I created it for headers like Xlib, which was last modified in the 90s and does not provide macros or functionality for making dynamic loading easier. Most modern headers provide macros to modify the declarations of functions, so compiler flags like __


