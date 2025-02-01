using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoomRenderer : MonoBehaviour
{
    [Header("Size in XY, 'Height' along Z")]
    public int sizeX = 8;
    public int sizeY = 8;
    public int sizeZ = 20;

    [Header("References")]
    public Polynomino4D polynomino; // So we can read polynomino.cubeSize

    [Header("Room Offset")]
    public Vector3 roomOffset = Vector3.zero;

    private Mesh lineMesh;

    // Rigidbody for each wall
    private Rigidbody[] wallRbs;

    void Start()
    {
        BuildLineMesh();
        wallRbs = new Rigidbody[5];
        // floor collider 
        wallRbs[0] = gameObject.AddComponent<Rigidbody>();
        wallRbs[0].isKinematic = true;
        wallRbs[0].useGravity = false;
        wallRbs[0].mass = 1000f;
        wallRbs[0].collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        wallRbs[0].interpolation = RigidbodyInterpolation.Interpolate;
        wallRbs[0].constraints = RigidbodyConstraints.FreezeAll;
        wallRbs[0].gameObject.GetComponent<BoxCollider>().isTrigger = false;
        //position the floor    
        wallRbs[0].transform.position = new Vector3(sizeX * polynomino.cubeSize / 2, sizeY * polynomino.cubeSize / 2, sizeZ * polynomino.cubeSize);
        wallRbs[0].transform.localScale = new Vector3(sizeX * polynomino.cubeSize, sizeY * polynomino.cubeSize, 1);
        //rotate the floor
        wallRbs[0].transform.Rotate(90, 0, 0);



    }

    private void BuildLineMesh()
    {
        if (polynomino == null)
        {
            Debug.LogError("RoomRenderer: polynomino reference is missing!");
            return;
        }

        float cSize = polynomino.cubeSize;

        var verts = new List<Vector3>();
        var indices = new List<int>();

        // Helper to add a line segment
        void AddLine(Vector3 a, Vector3 b)
        {
            int startIndex = verts.Count;
            verts.Add(a);
            verts.Add(b);
            indices.Add(startIndex);
            indices.Add(startIndex + 1);
        }

        // We treat z=0 as the "top," and z=sizeZ as the "floor."
        // We'll draw a "floor" plane at z=sizeZ, and walls
        // going from z=0 to z=sizeZ.

        // -----------------------------------------------------------
        // 1) Floor at z=sizeZ
        // -----------------------------------------------------------
        for (int x = 0; x <= sizeX; x++)
        {
            float X = x * cSize;
            Vector3 start = new Vector3(X, 0f, sizeZ * cSize) + roomOffset;
            Vector3 end = new Vector3(X, sizeY * cSize, sizeZ * cSize) + roomOffset;
            AddLine(start, end);
        }
        for (int y = 0; y <= sizeY; y++)
        {
            float Y = y * cSize;
            Vector3 start = new Vector3(0f, Y, sizeZ * cSize) + roomOffset;
            Vector3 end = new Vector3(sizeX * cSize, Y, sizeZ * cSize) + roomOffset;
            AddLine(start, end);
        }

        // -----------------------------------------------------------
        // 2) Walls around edges from z=0..z=sizeZ
        //    We'll put a line grid on each of the 4 sides:
        //    - x=0, y in [0..sizeY], z in [0..sizeZ]
        //    - x=sizeX, ...
        //    - y=0, ...
        //    - y=sizeY, ...
        // -----------------------------------------------------------

        // Wall A: x=0
        {
            float X = 0;
            // vertical lines along z
            for (int y = 0; y <= sizeY; y++)
            {
                float Y = y * cSize;
                Vector3 start = new Vector3(X, Y, 0f) + roomOffset;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) + roomOffset;
                AddLine(start, end);
            }
            // horizontal lines along y
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(X, 0f, Z) + roomOffset;
                Vector3 end = new Vector3(X, sizeY * cSize, Z) + roomOffset;
                AddLine(start, end);
            }
        }

        // Wall B: x=sizeX
        {
            float X = sizeX * cSize;
            for (int y = 0; y <= sizeY; y++)
            {
                float Y = y * cSize;
                Vector3 start = new Vector3(X, Y, 0f) + roomOffset;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) + roomOffset;
                AddLine(start, end);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(X, 0f, Z) + roomOffset;
                Vector3 end = new Vector3(X, sizeY * cSize, Z) + roomOffset;
                AddLine(start, end);
            }
        }

        // Wall C: y=0
        {
            float Y = 0;
            for (int x = 0; x <= sizeX; x++)
            {
                float X = x * cSize;
                Vector3 start = new Vector3(X, Y, 0f) + roomOffset;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) + roomOffset;
                AddLine(start, end);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(0f, Y, Z) + roomOffset;
                Vector3 end = new Vector3(sizeX * cSize, Y, Z) + roomOffset;
                AddLine(start, end);
            }
        }

        // Wall D: y=sizeY
        {
            float Y = sizeY * cSize;
            for (int x = 0; x <= sizeX; x++)
            {
                float X = x * cSize;
                Vector3 start = new Vector3(X, Y, 0f) + roomOffset;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) + roomOffset;
                AddLine(start, end);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(0f, Y, Z) + roomOffset;
                Vector3 end = new Vector3(sizeX * cSize, Y, Z) + roomOffset;
                AddLine(start, end);
            }
        }


        // Build the final mesh
        lineMesh = new Mesh { name = "RoomWireframe" };
        lineMesh.SetVertices(verts);
        lineMesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        lineMesh.RecalculateBounds();

        var mf = GetComponent<MeshFilter>();
        mf.sharedMesh = lineMesh;
    }
}
