using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoomRenderer : MonoBehaviour
{
    [Header("Room Dimensions (in cells)")]
    public int sizeX = 8;
    public int sizeY = 8;
    public int sizeZ = 20;

    [Header("References")]
    public Polynomino4D polynomino; // to get cubeSize
    // If you want an offset (move the room in space):
    public Vector3 roomOffset;

    private Mesh lineMesh;

    void Start()
    {
        BuildLineMesh();
    }

    /// <summary>
    /// Build the line mesh so we see the "room" as wireframe:
    /// - The floor is a grid in x,y at z=0
    /// - The 4 walls around the edges, from z=0..sizeZ
    /// - No top
    /// </summary>
    private void BuildLineMesh()
    {
        if (polynomino == null)
        {
            Debug.LogError("RoomRenderer: polynomino reference is missing!");
            return;
        }

        float cSize = polynomino.cubeSize;

        // We'll store the line vertices in a List<Vector3> and the indices in a List<int>
        var verts = new List<Vector3>();
        var indices = new List<int>();

        // Helper function to add a line from a->b
        void AddLine(Vector3 a, Vector3 b)
        {
            int startIndex = verts.Count;
            verts.Add(a);
            verts.Add(b);
            indices.Add(startIndex);
            indices.Add(startIndex + 1);
        }

        // SHIFT everything by roomOffset, to place the room in the world
        // We can do it once in the final transform, or we can add it to each vertex. 
        // We'll just add it to each vertex for simplicity.

        // -----------------------------------------------------------
        // 1) Floor grid (z=0 plane)
        // -----------------------------------------------------------
        // lines parallel to X for each Y in [0..sizeY]
        for (int y = 0; y <= sizeY; y++)
        {
            float Y = y * cSize;
            Vector3 start = new Vector3(0, Y, 0);
            Vector3 end = new Vector3(sizeX * cSize, Y, 0);
            AddLine(start + roomOffset, end + roomOffset);
        }
        // lines parallel to Y for each X in [0..sizeX]
        for (int x = 0; x <= sizeX; x++)
        {
            float X = x * cSize;
            Vector3 start = new Vector3(X, 0, 0);
            Vector3 end = new Vector3(X, sizeY * cSize, 0);
            AddLine(start + roomOffset, end + roomOffset);
        }

        // -----------------------------------------------------------
        // 2) 4 Walls (wireframe)
        //    We'll subdivide each wall in steps of 1 in the dimension's direction
        //    so it looks like a grid. 
        // -----------------------------------------------------------

        // Wall A: x=0, y in [0..sizeY], z in [0..sizeZ]
        {
            float X = 0;
            // vertical lines: for y in [0..sizeY], we go z=0..sizeZ
            for (int y = 0; y <= sizeY; y++)
            {
                float Y = y * cSize;
                Vector3 start = new Vector3(X, Y, 0);
                Vector3 end = new Vector3(X, Y, sizeZ * cSize);
                AddLine(start + roomOffset, end + roomOffset);
            }
            // horizontal lines (along y): for z in [0..sizeZ]
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(X, 0, Z);
                Vector3 end = new Vector3(X, sizeY * cSize, Z);
                AddLine(start + roomOffset, end + roomOffset);
            }
        }

        // Wall B: x=sizeX, y in [0..sizeY], z in [0..sizeZ]
        {
            float X = sizeX * cSize;
            for (int y = 0; y <= sizeY; y++)
            {
                float Y = y * cSize;
                Vector3 start = new Vector3(X, Y, 0);
                Vector3 end = new Vector3(X, Y, sizeZ * cSize);
                AddLine(start + roomOffset, end + roomOffset);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(X, 0, Z);
                Vector3 end = new Vector3(X, sizeY * cSize, Z);
                AddLine(start + roomOffset, end + roomOffset);
            }
        }

        // Wall C: y=0, x in [0..sizeX], z in [0..sizeZ]
        {
            float Y = 0;
            // vertical lines for x in [0..sizeX], z=0..sizeZ
            for (int x = 0; x <= sizeX; x++)
            {
                float X = x * cSize;
                Vector3 start = new Vector3(X, Y, 0);
                Vector3 end = new Vector3(X, Y, sizeZ * cSize);
                AddLine(start + roomOffset, end + roomOffset);
            }
            // horizontal lines (along x): for z in [0..sizeZ]
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(0, Y, Z);
                Vector3 end = new Vector3(sizeX * cSize, Y, Z);
                AddLine(start + roomOffset, end + roomOffset);
            }
        }

        // Wall D: y=sizeY, x in [0..sizeX], z in [0..sizeZ]
        {
            float Y = sizeY * cSize;
            for (int x = 0; x <= sizeX; x++)
            {
                float X = x * cSize;
                Vector3 start = new Vector3(X, Y, 0);
                Vector3 end = new Vector3(X, Y, sizeZ * cSize);
                AddLine(start + roomOffset, end + roomOffset);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(0, Y, Z);
                Vector3 end = new Vector3(sizeX * cSize, Y, Z);
                AddLine(start + roomOffset, end + roomOffset);
            }
        }

        // Done building line geometry. Now create the mesh.
        lineMesh = new Mesh();
        lineMesh.name = "RoomWireframe";
        lineMesh.SetVertices(verts);
        lineMesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        lineMesh.RecalculateBounds();

        // Assign to our MeshFilter
        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = lineMesh;
    }
}
