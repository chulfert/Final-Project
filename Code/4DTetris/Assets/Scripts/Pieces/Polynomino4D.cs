using UnityEngine;
using System.Collections.Generic;

public class Polynomino4D : MonoBehaviour
{
    public enum RotationAxis { XY, XZ, XW, YZ, YW, ZW }
    public enum MovementAxis { X, Y, Z }

    [Header("Prefab")]
    public GameObject Cube_Prefab;

    [Tooltip("Prefab that contains the 'Hypercube' script.")]
    public GameObject hypercubePrefab;

    // We'll store references to each Hypercube we spawn
    public List<Hypercube> hypercubes = new List<Hypercube>();
    public List<CubeRep> cubes = new List<CubeRep>();
    

    [Header("4D Rotation Angles (in degrees)")]
    public float rotationXY, rotationXZ, rotationXW, rotationYZ, rotationYW, rotationZW;

    [Header("References")]
    public GameObject board;
    private BoardState boardState;

    private Vector3 boardOrigin = Vector3.zero;
    private Vector3 boardExtends = Vector3.zero;

    public float cubeSize = 1.0f;
    void Start()
    {
        CreateStandardPolynomino();
        
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
                cube.SetRotation4D(rotationXY, rotationXZ, rotationXW, rotationYZ, rotationYW, rotationZW);
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

        // From the board get the position and size of the playfield
        // boardOrigin = boardState.GetBoardOrigin();
        //boardExtends = boardState.GetBoardExtends();
        board = GameObject.Find("Board");
        boardState = board.GetComponent<BoardState>();
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
                float oldX = targetPosition.x;
                targetPosition.x += direction ? cubeSize : -cubeSize;
                //Check the edges
                // conver the final position of the hypercubes
                // after the move into grid coordinates
                foreach (var hc in hypercubes)
                {
                    Vector3 pos = hc.GetPosition3D();
                    Vector3 targetPos = pos + new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                    if (!boardState.CheckBounds(targetPos))
                    {
                        targetPosition.x = oldX;
                    }
                }

                break;
            case MovementAxis.Y:
                float oldY = targetPosition.y;
                targetPosition.y += direction ? cubeSize : -cubeSize;
                foreach (var hc in hypercubes)
                {
                    Vector3 pos = hc.GetPosition3D();
                    Vector3 targetPos = pos + new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                    if (!boardState.CheckBounds(targetPos))
                    {
                        targetPosition.y = oldY;
                    }
                }
                break;
            case MovementAxis.Z:
                float oldZ = targetPosition.z;
                targetPosition.z += direction ? cubeSize : -cubeSize;
                foreach (var hc in hypercubes) {
                    Vector3 pos = hc.GetPosition3D();
                    Vector3 targetPos = pos + new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                    if (!boardState.CheckZBounds(targetPos))
                    {
                        boardState.TransferCubes(this);
                    }
                }
                break;
        }
    }

    public void addRotation(RotationAxis axis, bool direction)
    {
        targetRotation[(int)axis] += direction ? 90 : -90;
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
            AddHypercube(chosenOffsets[i]);
        };
    }

    /// Remove a specific hypercube from the polynomino, might not be needed TODO: check
    public void RemoveHypercube(Hypercube cube)
    {
        if (cube != null)
        {
            hypercubes.Remove(cube);
            Destroy(cube.gameObject);
        }
    }

    /// remove all the hypercubes, might be needed when we connect to the stack
    public void ClearAllHypercubes()
    {
        foreach (var hc in hypercubes)
        {
            if (hc != null) Destroy(hc.gameObject);
        }
        hypercubes.Clear();
    }

    /// Add a hyperbuve to the polynomino at a specific 4D offset.
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

        GameObject cubeGO = Instantiate(Cube_Prefab, hc.GetPosition3D(), Quaternion.identity);
        CubeRep cubeComponent = cubeGO.GetComponent<CubeRep>();
        cubes.Add(cubeComponent);
        cubeComponent.render = false;
        hc.linkedCube = cubeComponent;
        hc.transform.localScale = new Vector3(cubeSize, cubeSize, cubeSize);

        hypercubes.Add(hc);
        return hc;
    }
}


