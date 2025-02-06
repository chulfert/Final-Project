using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Polynomino4D manages a collection of Hypercubes that form a 4D shape.
/// - Builds itself from a template (array of 4D offsets).
/// - Keeps track of rotation angles in 4D (XY, XZ, XW, YZ, YW, ZW).
/// - Forwards those angles + each block's offset to the Hypercube scripts.
/// - Supports removing hypercubes (e.g. for Tetris line clears).
/// </summary>
public class Polynomino4D : MonoBehaviour
{
    public enum RotationAxis { XY, XZ, XW, YZ, YW, ZW }
    public enum MovementAxis { X, Y, Z }

    [Header("Template & Prefab")]
    [Tooltip("Which 4D shape do we use? (set of blockOffsets in 4D)")]
    public Polynomino4DTemplate template;
    public GameObject Cube_Prefab;

    [Tooltip("Prefab that contains the 'Hypercube' script.")]
    public GameObject hypercubePrefab;

    // We'll store references to each Hypercube we spawn
    private List<Hypercube> hypercubes = new List<Hypercube>();
    public List<CubeRep> cubes = new List<CubeRep>();
    

    [Header("4D Rotation Angles (in degrees)")]
    public float rotationXY, rotationXZ, rotationXW, rotationYZ, rotationYW, rotationZW;

    public float cubeSize = 1.0f;
    void Start()
    {
        BuildFromTemplate();
        // Random rotation for startup (in 90-degree increments)
        rotationXY = Random.Range(0, 4) * 90;
        rotationXZ = Random.Range(0, 4) * 90;
        rotationXW = Random.Range(0, 4) * 90;
        rotationYZ = Random.Range(0, 4) * 90;
        rotationYW = Random.Range(0, 4) * 90;
        rotationZW = Random.Range(0, 4) * 90;

        // Set the initial rotation for all Hypercubes
        foreach (var cube in hypercubes)
        {
            if (cube != null)
            {
                cube.SetRotation4D(
                                       rotationXY, rotationXZ, rotationXW,
                                                          rotationYZ, rotationYW, rotationZW
                                                                         );
            }
        }

        foreach (var hc in hypercubes)
        {
            GameObject cubeGO = Instantiate(Cube_Prefab, hc.GetPosition3D(), Quaternion.identity);
            CubeRep cubeComponent = cubeGO.GetComponent<CubeRep>();
            cubes.Add(cubeComponent);
            cubeComponent.render = false;
            hc.linkedCube = cubeComponent;
        }

        // Store the initial rotation angles for later use
        currentRotation[0] = rotationXY;
        currentRotation[1] = rotationXZ;
        currentRotation[2] = rotationXW;
        currentRotation[3] = rotationYZ;
        currentRotation[4] = rotationYW;
        currentRotation[5] = rotationZW;

        // Set the target rotation to the current rotation
        targetRotation[0] = rotationXY;
        targetRotation[1] = rotationXZ;
        targetRotation[2] = rotationXW;
        targetRotation[3] = rotationYZ;
        targetRotation[4] = rotationYW;
        targetRotation[5] = rotationZW;

        
        
    }

    public float[] targetRotation = new float[6];
    private float[] currentRotation = new float[6];

    private Vector3 targetPosition = Vector3.zero;

    void Update()
    {
        // Interpolate between current and target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2);

        // Interpolate between current and target rotation
        for (int i = 0; i < 6; i++)
        {
            currentRotation[i] = Mathf.Lerp(currentRotation[i], targetRotation[i], Time.deltaTime * 2);
        }

        //rotate the cubes
        foreach (var cube in hypercubes)
        {
            if (cube != null)
            {
                cube.SetRotation4D(currentRotation[0], currentRotation[1], currentRotation[2], currentRotation[3], currentRotation[4], currentRotation[5]);
                cube.linkedCube.transform.position = cube.GetPosition3D();
                
            }
        }

        
       
    }

    public void addMovement(MovementAxis axis, bool direction)
    {
        switch (axis)
        {
            case MovementAxis.X:
                targetPosition.x += direction ? cubeSize : -cubeSize;
                break;
            case MovementAxis.Y:
                targetPosition.y += direction ? cubeSize : -cubeSize;
                break;
            case MovementAxis.Z:
                targetPosition.z += direction ? cubeSize : -cubeSize;
                break;
        }
    }

    public void addRotation(RotationAxis axis, bool direction)
    {
        targetRotation[(int)axis] += direction ? 90 : -90;
    }

    /// <summary>
    /// Instantiates one Hypercube for each 4D offset in the template.
    /// Then store them in our 'hypercubes' list.
    /// </summary>
    private void BuildFromTemplate()
    {
        if (template == null || hypercubePrefab == null)
        {
            Debug.LogWarning("Polynomino4D: Missing template or prefab!");
            CreateStandardPolynomino();
            return;
        }

        ClearAllHypercubes(); // if we had any old ones

        for (int i = 0; i < template.blockOffsets.Length; i++)
        {
            Vector4 offset = template.blockOffsets[i];

            GameObject go = Instantiate(hypercubePrefab, this.transform);
            Hypercube hc = go.GetComponent<Hypercube>();
            if (hc == null)
            {
                Debug.LogError("Prefab missing Hypercube component!", go);
                Destroy(go);
                continue;
            }

            // Set the local offset so the hypercube knows where it is in 4D
            hc.localOffset4D = offset;

            // Also set the current rotation angles, so it starts correctly
            hc.SetRotation4D(
                rotationXY, rotationXZ, rotationXW,
                rotationYZ, rotationYW, rotationZW
            );

            // Set the size of the cube
            hc.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

            hypercubes.Add(hc);
        }
    }

    public void CreateStandardPolynomino()
    {
        // 1) Grab the list of shapes
        var allShapes = StandardPolynominoes4D.shapes;
        // 2) Pick one randomly
        int index = Random.Range(0, allShapes.Length);
        Vector4[] chosenOffsets = allShapes[index];

        // 3) Clear existing hypercubes if needed
        ClearAllHypercubes();

        // 4) Spawn hypercubes with these offsets
        for (int i = 0; i < chosenOffsets.Length; i++)
        {
            // We'll reuse your existing method "AddHypercube(Vector4 localOffset)"
            AddHypercube(chosenOffsets[i]);
        }

        for (int i = 0; i < chosenOffsets.Length; i++)
        {
            Vector4 offset = chosenOffsets[i];

            GameObject go = Instantiate(hypercubePrefab, this.transform);
            Hypercube hc = go.GetComponent<Hypercube>();
            if (hc == null)
            {
                Debug.LogError("Prefab missing Hypercube component!", go);
                Destroy(go);
                continue;
            }

            // Set the local offset so the hypercube knows where it is in 4D
            hc.localOffset4D = offset;

            // Also set the current rotation angles, so it starts correctly
            hc.SetRotation4D(
                rotationXY, rotationXZ, rotationXW,
                rotationYZ, rotationYW, rotationZW
            );

            // Set the size of the cube
            hc.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

            hypercubes.Add(hc);
        }

        Debug.Log($"Created standard polynomino index: {index}, with {chosenOffsets.Length} blocks.");
    }

    /// <summary>
    /// Removes a single hypercube from this polynomino (e.g., Tetris line clear).
    /// Destroys the GameObject, and removes it from the 'hypercubes' list.
    /// </summary>
    public void RemoveHypercube(Hypercube cube)
    {
        if (cube != null)
        {
            hypercubes.Remove(cube);
            Destroy(cube.gameObject);
        }
    }

    /// <summary>
    /// Remove ALL hypercubes (destroy them). Called if the entire piece is removed.
    /// </summary>
    public void ClearAllHypercubes()
    {
        foreach (var hc in hypercubes)
        {
            if (hc != null) Destroy(hc.gameObject);
        }
        hypercubes.Clear();
    }

    /// <summary>
    /// If you want to add an extra hypercube at runtime (rare in Tetris, but possible).
    /// localOffset is in 4D, referencing the polynomino's origin.
    /// </summary>
    public Hypercube AddHypercube(Vector4 localOffset)
    {
        if (hypercubePrefab == null) return null;

        GameObject go = Instantiate(hypercubePrefab, this.transform);
        Hypercube hc = go.GetComponent<Hypercube>();
        hc.localOffset4D = localOffset;
        hc.SetRotation4D(
            rotationXY, rotationXZ, rotationXW,
            rotationYZ, rotationYW, rotationZW
        );

        hypercubes.Add(hc);
        return hc;
    }
}


