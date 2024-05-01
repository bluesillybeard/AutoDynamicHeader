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

## Important things to know

autodynamic header looks for things that look like functions, meaning macros and other structures are ignored.
This means, pre-processing of the input file and post-processing of the output file is likely required.
To be specific, the following items are not handled correctly and/or are ignored:
    - Macros
    - calling convention
    - keywords (extern, static, etc)
    - pre-procesing statements (#include, #ifdef, etc)
    - Pretty much anything C++
    - probably a lot more

## TODO
- macros for adding things like calling conventions and other function declaration things eaiser
- C++ filtering (the #ifdef + extern "C" crap)
- fix edge cases with functions that return pointers
