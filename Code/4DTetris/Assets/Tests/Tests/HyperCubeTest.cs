using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
/*using Hypercube;

public class HyperCubeTest
{
    private GameObject testGO;
    private Hypercube hypercube;

    [SetUp]
    public void Setup()
    {
        // Create a temporary GameObject + Hypercube
        testGO = new GameObject("TestHypercube");
        hypercube = testGO.AddComponent<Hypercube>();

        // Ensure the needed components are on the GameObject
        testGO.AddComponent<MeshFilter>();
        testGO.AddComponent<MeshRenderer>();

        // Set any defaults you want for the test
        hypercube.size = 1f;
        // You can do forced Awake/Start calls if needed, but typically
        // UnityTest runner will handle calling them once you step into Play mode.
        // For editor-mode tests, you might want to call them directly:
        hypercube.Awake();
    }

    [TearDown]
    public void Teardown()
    {
        // Clean up after each test
        Object.DestroyImmediate(testGO);
    }

    [Test]
    public void Has16BaseVertices()
    {
        // Because baseVertices is private, we can either expose it
        // or use reflection. Below is reflection for demonstration.
        var baseVerticesField = typeof(Hypercube)
            .GetField("baseVertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Vector4[] baseVerts = (Vector4[])baseVerticesField.GetValue(hypercube);

        Assert.IsNotNull(baseVerts, "baseVertices array is null");
        Assert.AreEqual(16, baseVerts.Length, "Tesseract should have 16 corners");
    }

    /// <summary>
    /// Test that the hypercube's edge list has 32 edges after Awake().
    /// </summary>
    [Test]
    public void Has32Edges()
    {
        var edgesField = typeof(Hypercube)
            .GetField("tesseractEdges", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        int[][] edges = (int[][])edgesField.GetValue(hypercube);

        Assert.IsNotNull(edges, "tesseractEdges is null");
        Assert.AreEqual(32, edges.Length, "Tesseract should have 32 edges");
    }

    /// <summary>
    /// Verify that with zero rotation, the 'transformedVerts' match the 'baseVertices'.
    /// </summary>
    [Test]
    public void NoRotation_MatchesBaseVertices()
    {
        // Zero out angles
        hypercube.rotationXY = 0;
        hypercube.rotationXZ = 0;
        hypercube.rotationXW = 0;
        hypercube.rotationYZ = 0;
        hypercube.rotationYW = 0;
        hypercube.rotationZW = 0;

        // Manually call Update once to apply transformations
        hypercube.Update();

        // Reflection to get both baseVertices and transformedVerts
        var baseField = typeof(Hypercube)
            .GetField("baseVertices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var transField = typeof(Hypercube)
            .GetField("transformedVerts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Vector4[] baseVerts = (Vector4[])baseField.GetValue(hypercube);
        Vector4[] transVerts = (Vector4[])transField.GetValue(hypercube);

        for (int i = 0; i < baseVerts.Length; i++)
        {
            Assert.AreEqual(baseVerts[i], transVerts[i],
                $"Vertex {i} mismatch with zero rotation. Expected {baseVerts[i]}, got {transVerts[i]}");
        }
    }
}
*/