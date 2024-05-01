The headers in this directory are for testing:
- pinc.h is simple enough for the basic functionality
- Xlib.h is the header I originally made this program for, so testing it is a natural next step.
- gl.h is a more complex header, as it has macros for the calling convention. For now, to get it to work:
    - Use find&replace to calling convention from gl.h
    - Run through autodynamicheader
    - use find&replace to add calling convention back
    - go through and fix the handful of errors