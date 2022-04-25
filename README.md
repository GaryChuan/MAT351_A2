# MAT351-A2

This repository was created to accomplish assignment 2 for MAT 351:Quaternions, Interpolation, and Animation.

The assignment involves the completion of the following two tasks:

1. Create a program that reads an input file and outputs, using a visualization using an asymetrical object (such as the letter R) which displays the object at the indicated N + 1 positions Pi = (xi, yi) at time ti = i seconds, oriented appropriately, with initial velocity v0 = (v01, v02) at time t0 = 0, and final velocity vN = (vN1, vN2) at time tN = N. The object is assumed to be moving in a path with smooth acceleration. The input file will start with a line with an integer N, 1 <= N <= 10, ending in a carriage return, followed by a line two real numbers V01 V02, separated by a single space, ending in a carriage return. This will be followed by N + 1 lines, each with two real numbers xi yi separated by a single space ending in a carraige return. Finally, it will be followed by a line with two real numbers vN1 vN2, separated by a single space, with -10 <= xi, yi <= 10, -30 <= v0j, vNj <= 30, where 10xi, 10yi, 10v0j, 10vNj are integers.

2. Create a program that reads an input file and outputs, as a sequence of orientations ni1 ni2 ni3 θi, the N + 1 orientations of the object (fixing ni = <ni1, ni2, ni3> rotated by an angle of θi counterclockwise, 0 <= θi <= 360), which is at position Pi = <xi, yi, zi> at time ti i seconds, with initial velocity v0 = <v01, v02, v03> at time t0 = 0 and final velocity vN = <vN1, vN2, vN3> at time tN = N. This may be outputted to a file with one orientation per line sequentially.  The input file will start with a line with an integer N, 1 <= N <= 10, ending in a carraige return, followed by a line three real numbers V01 V02 V03, separated by a single space, ending in a carriage return. This will be followed by N + 1 lines, each with three real numbers xi yi zi separated by a single space ending in a carraige return. Finally, it will be followed by a line with three real numbers vN1 vN2 vN3, separated by a single space, with -10 <= xi, yi, zi <= 10, -30 <= v0j, vNj <= 30, where 10xi, 10yi, 10zi, 10v0j, 10vNj are integers.

A visualization using an asymmetrical object which displays all the intermediate positions and orientations for the problem, for time t, 0 ≤ t ≤ N, will be awarded five additional points

