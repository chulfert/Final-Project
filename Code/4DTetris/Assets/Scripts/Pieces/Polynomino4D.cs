using UnityEngine;
using System.Collections.Generic;

public class Polynomino4D : MonoBehaviour
{
    public enum RotationAxis { XY, XZ, XW, YZ, YW, ZW }
    public enum MovementAxis { X, Y, Z }

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

    float endPolyTimer = 0.0f;
    bool endPolyTimerStarted = false;

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

        // Store the initial rotation angles for later use
        currentRotation[0] = rotationXY;
        currentRotation[1] = rotationXZ;
        currentRotation[2] = rotationXW;
        currentRotation[3] = rotationYZ;
        currentRotation[4] = rotationYW;
        currentRotation[5] = rotationZW;

        // Set the target rotation to the current rotationa
        targetRotation[0] = rotationXY;
        targetRotation[1] = rotationXZ;
        targetRotation[2] = rotationXW;
        targetRotation[3] = rotationYZ;
        targetRotation[4] = rotationYW;
        targetRotation[5] = rotationZW;

        // From the board get the position and size of the playfield
        board = GameObject.Find("Board");

        boardState = board.GetComponent<BoardState>();
        boardOrigin = boardState.GetBoardOrigin();
        boardExtends = boardState.GetBoardExtends();
        targetPosition = transform.position;
    }

    public float[] targetRotation = new float[6];
    private float[] currentRotation = new float[6];

    public Vector3 targetPosition = Vector3.zero;

    void Update()
    {
        // Interpolate between current and target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2);

        // Interpolate between current and target rotation
        for (int i = 0; i < 6; i++)
        {
            currentRotation[i] = Mathf.Lerp(currentRotation[i], targetRotation[i], Time.deltaTime * 2);
        }
        


        //rotate the cubes // update the board
        foreach (var cube in hypercubes)
        {
            if (cube != null)
            {
                cube.SetRotation4D(currentRotation[0], currentRotation[1], currentRotation[2], currentRotation[3], currentRotation[4], currentRotation[5]);

            }
        }
        if (endPolyTimerStarted)
        {
            endPolyTimer += Time.deltaTime;

            // end the polyomino if it has been in the same position for 0.8 seconds
            if (endPolyTimer > 0.8f)
            {
                boardState.TransferCubes(this);
            }
        }
    }

    public void addMovement(MovementAxis axis, bool direction)
    {
        GameObject board = GameObject.Find("Board");
        board.GetComponent<BoardState>().ClearFalling();
        Vector3 oldPosition = targetPosition;
        Vector3 newPosition = targetPosition;
        switch (axis)
        {
            case MovementAxis.X:
                newPosition.x += direction ? cubeSize : -cubeSize;
                break;
            case MovementAxis.Y:
                newPosition.y += direction ? cubeSize : -cubeSize;
                break;
            case MovementAxis.Z:
                newPosition.z += direction ? cubeSize : -cubeSize;
                break;
        }

        bool canMove = true;
        bool hittingPiece = false;

        foreach (var hc in hypercubes)
        {
            if(!hc.IsVisible()) continue;
            Vector3 pos = hc.GetPosition3D();
            Vector3 targetPos = pos + newPosition;
            if(axis == MovementAxis.Z)
            {
                if (!boardState.CheckZBounds(targetPos))
                {
                    boardState.TransferCubes(this);
                    return;
                }
            }
            else
            {
                if (!boardState.CheckBounds(targetPos))
                {
                    canMove = false;
                    break;
                }
                
            }
            if (!boardState.CheckNextFree(targetPos))
            {
                hittingPiece = true;
                canMove = false;
                break;
            }
        }

        if (canMove) {
            targetPosition = newPosition;
            foreach (var hc in hypercubes)
            {
                if (!hc.IsVisible()) continue;
                Vector3 pos = hc.GetPosition3D();
                Vector3 targetPos = pos + targetPosition;
                board.GetComponent<BoardState>().SetFalling(targetPos);
            }
            endPolyTimerStarted = false;
        }
        else if (hittingPiece)
        {
            endPolyTimerStarted = true;
        }
        else
        {
            board.GetComponent<BoardState>().SetFalling(targetPosition);
        }
    


        /*
        switch (axis)
        {
            case MovementAxis.X:
                float oldX = targetPosition.x;
                targetPosition.x += direction ? cubeSize : -cubeSize;
                foreach (var hc in hypercubes)
                {
                    if(!hc.IsVisible()) continue;
                    Vector3 pos = hc.GetPosition3D();
                    Vector3 targetPos = pos + new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                    if (!boardState.CheckBounds(targetPos))
                    {
                        targetPosition.x = oldX;
                        board.GetComponent<BoardState>().SetFalling(targetPosition);
                        continue;
                    }
                    board.GetComponent<BoardState>().SetFalling(targetPos);
                }

                break;
            case MovementAxis.Y:
                float oldY = targetPosition.y;
                targetPosition.y += direction ? cubeSize : -cubeSize;
                foreach (var hc in hypercubes)
                {
                    if (!hc.IsVisible()) continue;
                    Vector3 pos = hc.GetPosition3D();
                    Vector3 targetPos = pos + new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                    if (!boardState.CheckBounds(targetPos))
                    {
                        targetPosition.y = oldY;
                        board.GetComponent<BoardState>().SetFalling(targetPosition + pos);
                        continue;
                    }                                
                    board.GetComponent<BoardState>().SetFalling(targetPos);
                }
                break;
            case MovementAxis.Z:
                Vector3 oldPosition = targetPosition;
                float oldZ = targetPosition.z;
                targetPosition.z += direction ? cubeSize : -cubeSize;
                foreach (var hc in hypercubes)
                {
                    if (!hc.IsVisible()) continue;
                    Vector3 pos = hc.GetPosition3D();
                    Vector3 targetPos = pos + new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
                    if (targetPos.z < 0) continue;
                    if (!boardState.CheckZBounds(targetPos))
                    {
                        boardState.TransferCubes(this);
                        return;
                    }

                    if (!boardState.CheckNextFree(targetPos))
                    {
                        targetPosition.z = oldZ;
                        board.GetComponent<BoardState>().SetFalling(targetPosition + pos);
                        endPolyTimerStarted = true;
                        break;
                    }
                    else
                    {
                        endPolyTimerStarted = false;
                        endPolyTimer = 0.0f;
                    }
                    board.GetComponent<BoardState>().SetFalling(targetPos);
                }
                break;
        }*/
    }

    public void addRotation(RotationAxis axis, bool direction)
    {
        if (FindAnyObjectByType<AudioManager>() != null)
        {
            AudioManager.Instance.PlayRotationSound();
        }

        float oldRotation = targetRotation[(int)axis];
        targetRotation[(int)axis] += direction ? 90 : -90;
        //Check if rotation is possible TODODODODO
        
        }

    public void CreateStandardPolynomino()
    {
        var allShapes = StandardPolynominoes4D.shapes;
        int index = Random.Range(0, allShapes.Length);
        Vector4[] chosenOffsets = allShapes[index];

        ClearAllHypercubes();

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
        hypercubes.Add(hc);
        return hc;
    }
}


