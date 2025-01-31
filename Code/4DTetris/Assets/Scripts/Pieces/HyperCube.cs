using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Hypercube : MonoBehaviour
{
    [Header("HyperCube Settings")]
    [Tooltip("Size of the hypercube in each dimension (XYZW).")]
    public float size = 1f;

    [Header("4D Rotation Angles (in degrees)")]
    public float rotationXY;
    public float rotationXZ;
    public float rotationXW;
    public float rotationYZ;
    public float rotationYW;
    public float rotationZW;

    [Header("4D Slice Parameter")]
    public float sliceW = 0f;

    [Header("Projection / Slice Settings")]
    [Tooltip("How we project from 4D to 3D (e.g. orthographic, perspective, etc.).")]
    public bool usePerspectiveProjection = false;
    public float perspectiveDistance = 5f;

    [Header("Auto Convert to 3D Cube?")]
    [Tooltip("If true, we gradually make w=0 once a condition is met.")]
    public bool convertToCube = false;
    public float wThreshold = 0.1f;   // threshold below which w is forced to 0

    // -- Internal data structures --
    private Vector4[] baseVertices;      // The 16 base coordinates of a tesseract
    private Vector4[] transformedVerts;  // The transformed 4D coordinates after rotation
    private Vector3[] projectedVerts;    // The final 3D vertices for rendering

    private Mesh mesh;

    // The local offset in 4D, as assigned by the Polynomino
    public Vector4 localOffset4D = Vector4.zero;
    public Vector4 rotatedOffset = Vector4.zero;


    // -- Constants for tesseract geometry --    
    private static int[][] tesseractEdges = new int[][] {};

    void Awake()
    {
        // 1) Create the base vertex set for a tesseract
        CreateBaseVertices();

        // 2) Create the edge table (or do it manually/hard-coded)
        CreateEdgeTable();

        // 3) Initialize the Mesh
        mesh = new Mesh();
        mesh.name = "HypercubeMesh";
        GetComponent<MeshFilter>().mesh = mesh;

        transformedVerts = new Vector4[baseVertices.Length];
        projectedVerts = new Vector3[baseVertices.Length];
    }

    void Update()
    {
        // 1) Rotate + transform the 4D vertices
        Apply4DRotation();

        // 2) If we’re converting to a 3D cube, clamp w near zero
        if (convertToCube)
        {
            ConvertTo3DIfThreshold();
        }

        // 3) Project from 4D -> 3D 
        ProjectVertices();

        // 4) Update the mesh
        UpdateMesh();
    }

    // ------------------------------------------------------------------------
    // STEP 1) CREATE BASE VERTICES (16 corners of a 4D hypercube)
    // ------------------------------------------------------------------------
    private void CreateBaseVertices()
    {
        // A tesseract has 16 vertices: each coordinate is +size/2 or -size/2
        float half = size * 0.5f;
        // 16 vertices means 2^4 combinations
        baseVertices = new Vector4[16];

        int index = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    for (int w = -1; w <= 1; w += 2)
                    {
                        baseVertices[index++] = new Vector4(
                            x * half,
                            y * half,
                            z * half,
                            w * half
                        );
                    }
                }
            }
        }
    }

    private void CreateEdgeTable()
    {
        // Because we know the structure of a tesseract, we can systematically link edges:
        // In a 4D hypercube, two vertices form an edge if they differ in exactly one coordinate.
        // This function can be done programmatically to avoid mistakes.

        // We'll store the edges in tesseractEdges as pairs [v1, v2].
        // For simplicity, do it in a single pass:
        var edges = new System.Collections.Generic.List<int[]>();
        for (int i = 0; i < baseVertices.Length; i++)
        {
            for (int j = i + 1; j < baseVertices.Length; j++)
            {
                Vector4 vi = baseVertices[i];
                Vector4 vj = baseVertices[j];

                // Count how many coordinates differ
                int diffCount = 0;
                if (Mathf.Abs(vi.x - vj.x) > 0.001f) diffCount++;
                if (Mathf.Abs(vi.y - vj.y) > 0.001f) diffCount++;
                if (Mathf.Abs(vi.z - vj.z) > 0.001f) diffCount++;
                if (Mathf.Abs(vi.w - vj.w) > 0.001f) diffCount++;

                if (diffCount == 1)
                {
                    edges.Add(new int[] { i, j });
                }
            }
        }
        tesseractEdges = edges.ToArray();
    }

    // ------------------------------------------------------------------------
    // STEP 3) APPLY 4D ROTATION
    // ------------------------------------------------------------------------
    private void Apply4DRotation()
    {
        // Convert angles to radians
        float rxxy = rotationXY * Mathf.Deg2Rad;
        float rxxz = rotationXZ * Mathf.Deg2Rad;
        float rxxw = rotationXW * Mathf.Deg2Rad;
        float ryyz = rotationYZ * Mathf.Deg2Rad;
        float ryyw = rotationYW * Mathf.Deg2Rad;
        float rzzw = rotationZW * Mathf.Deg2Rad;

        // Build up a combined rotation matrix (4x4)
        Matrix4x4 rotMatrix = Matrix4x4.identity;
        rotMatrix = RotateXY(rotMatrix, rxxy);
        rotMatrix = RotateXZ(rotMatrix, rxxz);
        rotMatrix = RotateXW(rotMatrix, rxxw);
        rotMatrix = RotateYZ(rotMatrix, ryyz);
        rotMatrix = RotateYW(rotMatrix, ryyw);
        rotMatrix = RotateZW(rotMatrix, rzzw);

        // Apply to each base vertex
        for (int i = 0; i < baseVertices.Length; i++)
        {
            transformedVerts[i] = rotMatrix.MultiplyPoint4x4(baseVertices[i]);
            //Calculate the rotated offset
            rotatedOffset = rotMatrix.MultiplyPoint4x4(localOffset4D);


            transformedVerts[i] += rotatedOffset;
        }
    }

    // ----------- Helpers for rotating in 4D planes -----------------
    // XY-plane rotation
    private Matrix4x4 RotateXY(Matrix4x4 mat, float angle)
    {
        // Rotates in XY-plane, leaves Z and W alone
        Matrix4x4 m = Matrix4x4.identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        m[0, 0] = cos; m[0, 1] = -sin;
        m[1, 0] = sin; m[1, 1] = cos;

        return m * mat;
    }
    private Matrix4x4 RotateXZ(Matrix4x4 mat, float angle)
    {
        // Rotates in XZ-plane, leaves Y and W alone
        Matrix4x4 m = Matrix4x4.identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        m[0, 0] = cos; m[0, 2] = -sin;
        m[2, 0] = sin; m[2, 2] = cos;

        return m * mat;
    }
    private Matrix4x4 RotateXW(Matrix4x4 mat, float angle)
    {
        // Rotates in XW-plane, leaves Y and Z alone
        Matrix4x4 m = Matrix4x4.identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        m[0, 0] = cos; m[0, 3] = -sin;
        m[3, 0] = sin; m[3, 3] = cos;

        return m * mat;
    }
    private Matrix4x4 RotateYZ(Matrix4x4 mat, float angle)
    {
        // Rotates in YZ-plane, leaves X and W alone
        Matrix4x4 m = Matrix4x4.identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        m[1, 1] = cos; m[1, 2] = -sin;
        m[2, 1] = sin; m[2, 2] = cos;

        return m * mat;
    }
    private Matrix4x4 RotateYW(Matrix4x4 mat, float angle)
    {
        // Rotates in YW-plane, leaves X and Z alone
        Matrix4x4 m = Matrix4x4.identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        m[1, 1] = cos; m[1, 3] = -sin;
        m[3, 1] = sin; m[3, 3] = cos;

        return m * mat;
    }
    private Matrix4x4 RotateZW(Matrix4x4 mat, float angle)
    {
        // Rotates in ZW-plane, leaves X and Y alone
        Matrix4x4 m = Matrix4x4.identity;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        m[2, 2] = cos; m[2, 3] = -sin;
        m[3, 2] = sin; m[3, 3] = cos;

        return m * mat;
    }
    // ------------------------------------------------------------------------

    // ------------------------------------------------------------------------
    // STEP 4) OPTIONAL: CONVERT W -> 0 IF BELOW THRESHOLD
    //         This effectively collapses the hypercube to a 3D cube.
    // ------------------------------------------------------------------------
    private void ConvertTo3DIfThreshold()
    {
        for (int i = 0; i < transformedVerts.Length; i++)
        {
            var v = transformedVerts[i];
            if (Mathf.Abs(v.w) < wThreshold)
            {
                v.w = 0f;
                transformedVerts[i] = v;
            }
        }
    }

    // ------------------------------------------------------------------------
    // STEP 5) PROJECT 4D POINTS TO 3D
    // ------------------------------------------------------------------------
    private void ProjectVertices()
    {
        for (int i = 0; i < transformedVerts.Length; i++)
        {
            // Create a copy of the 4D vertex
            Vector4 v4 = transformedVerts[i];

            // SHIFT by sliceW in the w-dimension
            // i.e., if sliceW>0, we effectively move the hypercube so that
            // w= sliceW ends up at w=0 in the new, "shifted" coordinates
            v4.w -= sliceW;

            if (usePerspectiveProjection)
            {
                // Example perspective: treat w like part of the "camera distance"
                float dist = perspectiveDistance - v4.w;
                if (Mathf.Approximately(dist, 0f)) dist = 0.0001f;
                float invDist = 1.0f / dist;

                projectedVerts[i] = new Vector3(
                    v4.x * invDist,
                    v4.y * invDist,
                    v4.z * invDist
                );
            }
            else
            {
                // Orthographic: ignoring w
                projectedVerts[i] = new Vector3(v4.x, v4.y, v4.z);
            }
        }
    }

    public void SetRotation4D(
        float rXY, float rXZ, float rXW,
        float rYZ, float rYW, float rZW
    )
    {
        rotationXY = rXY;
        rotationXZ = rXZ;
        rotationXW = rXW;
        rotationYZ = rYZ;
        rotationYW = rYW;
        rotationZW = rZW;
    }

    // ------------------------------------------------------------------------
    // STEP 6) UPDATE MESH
    //         Here we’ll build a wireframe by forming lines between edges.
    //         Alternatively, you can build the full set of faces if you want
    //         a “solid” tesseract.
    // ------------------------------------------------------------------------
    private void UpdateMesh()
    {
        // For a wireframe approach using a standard Mesh in Unity,
        // we can represent each edge as a pair of vertices in the mesh, 
        // with indices that form lines. 
        // Another option: use a LineRenderer with multiple segments.

        // -- Build wireframe data --
        var wireVerts = new System.Collections.Generic.List<Vector3>();
        var wireIndices = new System.Collections.Generic.List<int>();

        for (int e = 0; e < tesseractEdges.Length; e++)
        {
            int i0 = tesseractEdges[e][0];
            int i1 = tesseractEdges[e][1];

            // Add both endpoints
            wireVerts.Add(projectedVerts[i0]);
            wireVerts.Add(projectedVerts[i1]);

            // Add line indices
            wireIndices.Add(wireVerts.Count - 2);
            wireIndices.Add(wireVerts.Count - 1);
        }

        mesh.Clear();

        // Because we’re building line geometry:
        mesh.SetVertices(wireVerts);
        mesh.SetIndices(wireIndices.ToArray(), MeshTopology.Lines, 0);

        // Normals/UVs are not critical for a wireframe. If you want a solid mesh,
        // you’ll need to define triangles and normals.

        mesh.RecalculateBounds();
    }
}


