using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoomRenderer : MonoBehaviour
{
    [Header("Size in XY, 'Height' along Z")]
    public int sizeX = 4;
    public int sizeY = 4;
    public int sizeZ = 10;

    [Header("References")]
    public Polynomino4D polynomino; 

    [Header("Room Offset")]

    private Mesh lineMesh;

    public Material fillMat;
    private Mesh quadMesh;
    private float cSize;
    void Start()
    {
        cSize = GameObject.Find("GameManager").GetComponent<PolyManager>().cubeSize;
        GameObject board = GameObject.Find("Board");
        board.transform.position = new Vector3(-sizeX / 2, -sizeY / 2, 0) * cSize;
        BuildLineMesh();
        CreateQuadMesh();
    }

    private void BuildLineMesh()
    {
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
   
        // -----------------------------------------------------------
        // 1) Floor at z=sizeZ
        // -----------------------------------------------------------
        for (int x = 0; x <= sizeX; x++)
        {
            float X = x * cSize;
            Vector3 start = new Vector3(X, 0f, sizeZ * cSize) ;
            Vector3 end = new Vector3(X, sizeY * cSize, sizeZ * cSize) ;
            AddLine(start, end);
        }
        for (int y = 0; y <= sizeY; y++)
        {
            float Y = y * cSize;
            Vector3 start = new Vector3(0f, Y, sizeZ * cSize) ;
            Vector3 end = new Vector3(sizeX * cSize, Y, sizeZ * cSize) ;
            AddLine(start, end);
        }

        // -----------------------------------------------------------
        // 2) Walls around edges from z=0..z=sizeZ
        //
        // Wall A: x=0
        {
            float X = 0;
            // vertical lines along z
            for (int y = 0; y <= sizeY; y++)
            {
                float Y = y * cSize;
                Vector3 start = new Vector3(X, Y, 0f) ;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) ;
                AddLine(start, end);
            }
            // horizontal lines along y
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(X, 0f, Z) ;
                Vector3 end = new Vector3(X, sizeY * cSize, Z) ;
                AddLine(start, end);
            }
        }

        // Wall B: x=sizeX
        {
            float X = sizeX * cSize;
            for (int y = 0; y <= sizeY; y++)
            {
                float Y = y * cSize;
                Vector3 start = new Vector3(X, Y, 0f) ;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) ;
                AddLine(start, end);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(X, 0f, Z) ;
                Vector3 end = new Vector3(X, sizeY * cSize, Z) ;
                AddLine(start, end);
            }
        }

        // Wall C: y=0
        {
            float Y = 0;
            for (int x = 0; x <= sizeX; x++)
            {
                float X = x * cSize;
                Vector3 start = new Vector3(X, Y, 0f) ;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) ;
                AddLine(start, end);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(0f, Y, Z) ;
                Vector3 end = new Vector3(sizeX * cSize, Y, Z) ;
                AddLine(start, end);
            }
        }

        // Wall D: y=sizeY
        {
            float Y = sizeY * cSize;
            for (int x = 0; x <= sizeX; x++)
            {
                float X = x * cSize;
                Vector3 start = new Vector3(X, Y, 0f) ;
                Vector3 end = new Vector3(X, Y, sizeZ * cSize) ;
                AddLine(start, end);
            }
            for (int z = 0; z <= sizeZ; z++)
            {
                float Z = z * cSize;
                Vector3 start = new Vector3(0f, Y, Z) ;
                Vector3 end = new Vector3(sizeX * cSize, Y, Z) ;
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
    void CreateQuadMesh()
    {
        quadMesh = new Mesh();
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 1, 0)
        };

        int[] tris = new int[]
        {
            0, 2, 1,
            0, 3, 2
        };

        quadMesh.vertices = vertices;
        quadMesh.triangles = tris;
        quadMesh.RecalculateNormals();
    }
    void OnRenderObject()
    {
        if (fillMat == null || quadMesh == null)
        {
            Debug.LogError("Missing fill material or quad mesh!");
            return;
        }
        fillMat.SetPass(0);

        // Retrieve the falling cubes (grid coordinates) from the Board.
        GameObject board = GameObject.Find("Board");
        List<Vector3Int> fallingCubes = board.GetComponent<BoardState>().GetFallingCubes();
        Vector3 board_pos = board.transform.position;
        // For each falling polynomino cube, draw a filled quad on all four walls.
        foreach (Vector3 cube in fallingCubes)
        {
            // --- Left Wall (x = 0) ---
            // Use board's Z for vertical (falling) and Y for horizontal.
            // Rotate -90° about Y so that the quad's local X (width) maps to world Z.
            Matrix4x4 matrixLeft = Matrix4x4.TRS(
            new Vector3(0, cube.y * cSize, cube.z * cSize) + board_pos,
            Quaternion.Euler(0, -90, 0),
            new Vector3(cSize, cSize, 1)
        );
            Graphics.DrawMeshNow(quadMesh, matrixLeft);

            // --- Right Wall (x = sizeX * cSize) ---
            Matrix4x4 matrixRight = Matrix4x4.TRS(
                new Vector3(sizeX * cSize, cube.y * cSize, cube.z * cSize) + board_pos,
                Quaternion.Euler(0, 90, 0),
                new Vector3(cSize, cSize, 1)
            );
            Graphics.DrawMeshNow(quadMesh, matrixRight);

            // --- Back Wall (y = 0) ---
            // Use board's X for horizontal and Z for vertical.
            // Rotate -90° about X so that local Y becomes world Z.
            Matrix4x4 matrixBack = Matrix4x4.TRS(
                new Vector3(cube.x * cSize, 0, cube.z * cSize) + board_pos,
                Quaternion.Euler(90, 0, 0),
                new Vector3(cSize, cSize, 1)
            );
            Graphics.DrawMeshNow(quadMesh, matrixBack);

            // --- Front Wall (y = sizeY * cSize) ---
            Matrix4x4 matrixFront = Matrix4x4.TRS(
                new Vector3(cube.x * cSize, sizeY * cSize, cube.z * cSize) + board_pos,
                Quaternion.Euler(-90, 0, 0),
                new Vector3(cSize, cSize, 1)
            );
            Graphics.DrawMeshNow(quadMesh, matrixFront);
        }
    }
}
