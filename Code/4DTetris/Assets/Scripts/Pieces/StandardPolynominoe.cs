using UnityEngine;
using System.Collections.Generic;

public static class StandardPolynominoes4D
{
    /// <summary>
    /// Around 20 different 4D polynomino shapes.
    /// Each shape is a set of connected (x,y,z,w) offsets.
    /// </summary>
    public static readonly Vector4[][] shapes = new Vector4[][]
    {
        // 1) A 4D "line" in x dimension (4 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(3,0,0,0),
        },

        // 2) A 2D L shape in x-y plane (4 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,2,0,0),
        },

        // 3) A 2D T shape in x-y plane (4 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(1,1,0,0),
        },

        // 4) A small "square" 2x2 in x-y plane
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(1,1,0,0),
        },

        // 5) A "plus" shape in x-y plane (5 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(1,1,0,0),
            new Vector4(1,-1,0,0),
        },

        // 6) A 3D "L" shape in x-y-z (5 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(0,0,1,0),
            new Vector4(0,1,0,0),
        },

        // 7) A 3D "T" shape in x-y-z (5 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(1,1,0,0),
            new Vector4(1,0,1,0),
        },

        // 8) A simple "cube corner" in x-y-z (4 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,0,1,0),
        },

        // 9) "S" shape in x-y plane (4 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(1,1,0,0),
            new Vector4(2,1,0,0),
        },

        // 10) "Z" shape in x-y plane (4 cells)
        new Vector4[]
        {
            new Vector4(0,1,0,0),
            new Vector4(1,1,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
        },

        // 11) A "3D plus" (cross in x,y,z)  (7 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(-1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,-1,0,0),
            new Vector4(0,0,1,0),
            new Vector4(0,0,-1,0),
        },

        // 12) A "4D line" spanning w dimension (4 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(0,0,0,1),
            new Vector4(0,0,0,2),
            new Vector4(0,0,0,3),
        },

        // 13) 2D "L" in x-w plane
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(0,0,0,1),
        },

        // 14) A 3D "square face" extended in w
        // A 2x2 block in x-y plane, plus shift in w
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(1,1,0,0),
            // shift entire square in w=1
            new Vector4(0,0,0,1),
            new Vector4(1,0,0,1),
            new Vector4(0,1,0,1),
            new Vector4(1,1,0,1),
        },

        // 15) 4D "T" shape: a T in x-y plus an extra block in w
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(1,1,0,0),
            new Vector4(1,0,0,1), // block in w dimension
        },

        // 16) "Stair" shape in x,y,w
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(2,1,0,0),
            new Vector4(2,1,0,1),
        },

        // 17) 2D "U" shape in x-y (5 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,2,0,0),
            new Vector4(1,0,0,0),
            new Vector4(1,2,0,0),
        },

        // 18) "Double L" in x-y (6 cells)
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(2,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,2,0,0),
            new Vector4(1,2,0,0),
        },

        // 19) A "4D cross": plus shape in x,y plus an extra dimension in w
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(-1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,-1,0,0),
            // now add one in w dimension
            new Vector4(0,0,0,1),
        },

        // 20) "Box corner" extended in w
        // a corner of a 3D cube plus w=1 for one block
        new Vector4[]
        {
            new Vector4(0,0,0,0),
            new Vector4(1,0,0,0),
            new Vector4(0,1,0,0),
            new Vector4(0,0,1,0),
            new Vector4(0,0,0,1),
        },
    };
}
