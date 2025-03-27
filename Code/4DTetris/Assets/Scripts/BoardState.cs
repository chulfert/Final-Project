using UnityEngine;
using System.Collections.Generic;

public class BoardState : MonoBehaviour
{
    // References
    [Header("References")]
    public RoomRenderer roomRenderer;
    public Polynomino4D current_polynomino;
    public GameObject basicCube;

    // States of each cell
    public enum CellState
    {
        Empty,
        Filled,
        Falling,
    }

    public struct Cell
    {
        public CellState state;
        public GameObject cube;
    }
    public struct Layer
    {
        public Cell[,] cells;
    }

    public List<Layer> board;

    public List<Color> colors = new List<Color>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeBoard();
    }

    public void InitializeBoard()
    {
        roomRenderer = GetComponent<RoomRenderer>();
        // Initialize the board state
        board = new List<Layer>();
        int board_height = roomRenderer.sizeZ;
        int board_x = roomRenderer.sizeX;
        int board_y = roomRenderer.sizeY;

        for (int i = 0; i < board_height; i++)
        {
            Layer layer = new Layer();
            layer.cells = new Cell[board_x, board_y];
            for (int j = 0; j < board_x; j++)
            {
                for (int k = 0; k < board_y; k++)
                {
                    layer.cells[j, k].state = CellState.Empty;
                    layer.cells[j, k].cube = null;
                }
            }
            board.Add(layer);
        }

        for (int i = 0; i < roomRenderer.sizeZ; i++)
        {
            colors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckLayersForFull();
    }

    public List<Vector3Int> GetFallingCubes()
    {
        List<Vector3Int> fallingCubes = new List<Vector3Int>();
        for (int z = 0; z < board.Count; z++)
        {
            for (int x = 0; x < board[z].cells.GetLength(0); x++)
            {
                for (int y = 0; y < board[z].cells.GetLength(1); y++)
                {
                    if (board[z].cells[x, y].state == CellState.Falling)
                    {
                        fallingCubes.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }
        return fallingCubes;
    }


    // From a given position, check if this is available
    public bool CheckNextFree(Vector3 targetPosition)
    {
        Vector3Int index = WorldToBoardIndex(targetPosition);
        if (index.z < 0) return true; // because we might spawn at negative z
        if (!CheckValidBoardPosition(index))
        {
            Debug.Log("Out of bounds error"); // TODO handle this
            return false;
        }
        if (board[index.z].cells[index.x, index.y].state == CellState.Filled)
        {
            return false;
        }
        return true;
    }

    public bool CheckBounds(Vector3 target)
    {        
        Vector3Int boardPosition = WorldToBoardIndex(target);
        if (boardPosition.x < 0 || boardPosition.x >= GetBoardExtends().x) return false;
        if (boardPosition.y < 0 || boardPosition.y >= GetBoardExtends().y) return false;
        return true;
    }

    public bool CheckZBounds(Vector3 target)
    {
        if (Mathf.Floor(target.z) >= GetBoardExtends().z * current_polynomino.cubeSize) return false;
        return true;
    }
    public Vector3 GetBoardOrigin()
    {
        // find GO Board
        GameObject go = GameObject.Find("Board");
        if(!go) return Vector3.zero; //For testing
        // get the size of the board    
        return go.transform.position;
    }
    public Vector3 GetBoardExtends()
    {
        // get the size of the board
        return new Vector3(roomRenderer.sizeX, roomRenderer.sizeY, roomRenderer.sizeZ);
    }

    public void TransferCubes(Polynomino4D polynomino)
    {
        if (FindAnyObjectByType<AudioManager>() != null)
        {
            AudioManager.Instance.PlayBlockPlacementSound();
        }
        int cubeCount = 0;
        // Check the hypercubes of the polynominoe, calculate the position in the board grid and set it to filled
        foreach (var cube in polynomino.hypercubes)
        {
            if (!cube.IsVisible()) continue;
            Vector3 pos = cube.GetPosition3D() + polynomino.targetPosition;
            Vector3Int index = WorldToBoardIndex(pos);
            cubeCount += board.Count - index.z;
            if (!CheckValidBoardPosition(index))
            {
                GameObject.Find("GameManager").GetComponent<GameStateManager>().GameOver();
                return;
            }
            board[index.z].cells[index.x, index.y].state = CellState.Filled;

            // Create a cube from the basic cube prefab and set it to this position
            Vector3 pos3 = BoardIndexToWorld(index);

            GameObject go = Instantiate(basicCube, pos3, Quaternion.identity);
            go.transform.localScale = new Vector3(polynomino.cubeSize, polynomino.cubeSize, polynomino.cubeSize);
            board[index.z].cells[index.x, index.y].cube = go;

            // Assign color based on z heioght
            Color c = colors[index.z];
            go.GetComponent<MeshRenderer>().material.color = c;

        }
        Destroy(polynomino.gameObject);
        GameObject gm = GameObject.Find("GameManager");
        gm.GetComponent<GameStateManager>().AddScore(cubeCount);
        gm.GetComponent<PolyManager>().SpawnNewPolynomino();
    }

    public void CheckLayersForFull()
    {
        for (int i = 0; i < board.Count; i++)
        {
            bool full = true;
            for (int j = 0; j < board[i].cells.GetLength(0); j++)
            {
                for (int k = 0; k < board[i].cells.GetLength(1); k++)
                {
                    if (board[i].cells[j, k].state == CellState.Empty || board[i].cells[j, k].state == CellState.Falling)
                    {
                        full = false;
                        break;
                    }
                }
                if (!full)
                {
                    break;
                }
            }
            if (full)
            {
                ClearLayer(i); // should rather return a list of ints and clear them somewhere else
            }
        }

    }

    public void ClearLayer(int layer)
    {
        if (FindAnyObjectByType<AudioManager>() != null)
        {
            AudioManager.Instance.PlayLevelClearSound();
        }


        // Find the GameMananger and update the score
        int score = board[0].cells.GetLength(0) * board[0].cells.GetLength(1) * 100;
        GameObject go = GameObject.Find("GameManager");
        if(go)
            go.GetComponent<GameStateManager>().AddScore(score);

        // destroy the full layer and move everything at a lower Z towards z by 1, spawn a new layer at the top
        for (int x = 0; x < board[layer].cells.GetLength(0); x++)
        {
            for (int y = 0; y < board[layer].cells.GetLength(1); y++)
            {
                if (board[layer].cells[x, y].cube != null)
                {
                    GameObject.Destroy(board[layer].cells[x, y].cube);
                    // We must update the actual struct in the array
                    Cell cell = board[layer].cells[x, y];
                    cell.state = CellState.Empty;
                    cell.cube = null;
                    board[layer].cells[x, y] = cell; // Assign back to update the actual data
                }
            }
        }
        for (int z = layer; z > 0; z--)
        {
            for (int x = 0; x < board[z].cells.GetLength(0); x++)
            {
                for (int y = 0; y < board[z].cells.GetLength(1); y++)
                {
                    // skip falling cubes
                    if (board[z].cells[x, y].state == CellState.Falling) continue;
                    // Copy the state from the layer above
                    board[z].cells[x, y].state = board[z - 1].cells[x, y].state;
                    board[z].cells[x, y].cube = board[z - 1].cells[x, y].cube;

                    // If there's a cube, move it up by 1 unit
                    if (board[z].cells[x, y].cube != null)
                    {
                        // Move the actual GameObject
                        Vector3 pos = board[z].cells[x, y].cube.transform.position;
                        pos.z = pos.z + current_polynomino.cubeSize; 
                        board[z].cells[x, y].cube.transform.position = pos;
                    }
                }
            }
        }
        // Resort the colors
        colors.RemoveAt(layer);
        colors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)));

    }
    public void ClearFalling()
    {
        for (int i = 0; i < board.Count; i++)
        {
            for (int j = 0; j < board[i].cells.GetLength(0); j++)
            {
                for (int k = 0; k < board[i].cells.GetLength(1); k++)
                {
                    if (board[i].cells[j, k].state == CellState.Falling)
                    {
                        board[i].cells[j, k].state = CellState.Empty;
                    }
                }

            }
        }
    }
    public void SetFalling(Vector3 pos_in)
    {
        Vector3Int pos = WorldToBoardIndex(pos_in);
        if (pos.z < 0) return;
        if (!CheckValidBoardPosition(pos)) return;
        board[pos.z].cells[pos.x, pos.y].state = CellState.Falling;
    }
    public Vector3Int WorldToBoardIndex(Vector3 pos)
    {
        Vector3 ext = GetBoardExtends();

        int x_round = Mathf.RoundToInt(pos.x);
        int y_round = Mathf.RoundToInt(pos.y);
        int z_round = Mathf.RoundToInt(pos.z);


        int x = Mathf.CeilToInt(x_round + ext.x / 2f);
        int y = Mathf.CeilToInt(y_round + ext.y / 2f);
        int z = Mathf.CeilToInt(z_round) - 1;
        return new Vector3Int(x, y, z);
    }
    public Vector3 BoardIndexToWorld(Vector3Int index)
    {
        Vector3 ext = GetBoardExtends();
        float x = (index.x - ext.x / 2f);
        float y = (index.y - ext.y / 2f);
        float z = index.z + 1;
        return new Vector3(x, y, z);
    }

    public bool CheckValidBoardPosition(Vector3Int index)
    {
        if (index.z < 0 || index.z >= board.Count)
            return false;

        if (index.x < 0 || index.x >= board[index.z].cells.GetLength(0))
            return false;

        if (index.y < 0 || index.y >= board[index.z].cells.GetLength(1))
            return false;

        return true;
    }

    public Vector3 ResetVector(Vector3 pos)
    {
        // Calculate how far out of bounds we are
        Vector3Int index = WorldToBoardIndex(pos);
        Vector3 mod = Vector3.zero;
        if (index.x < 0)
        {
            mod.x = -index.x;
        }
        if (index.x >= GetBoardExtends().x)
        {
            mod.x = GetBoardExtends().x - index.x - 1;
        }
        if (index.y < 0)
        {
            mod.y = -index.y;
        }
        if (index.y >= GetBoardExtends().y)
        {
            mod.y = GetBoardExtends().y - index.y - 1;
        }
        if (index.z < -1)
        {
            mod.z = -index.z;
        }
        if (index.z >= GetBoardExtends().z - 1)
        {
            mod.z = GetBoardExtends().z - index.z - 1;
        }
        return mod;


    }

    public void ResetBoard()
    {
        // Clear all existing cubes
        for (int z = 0; z < board.Count; z++)
        {
            for (int x = 0; x < board[z].cells.GetLength(0); x++)
            {
                for (int y = 0; y < board[z].cells.GetLength(1); y++)
                {
                    if (board[z].cells[x, y].cube != null)
                    {
                        Destroy(board[z].cells[x, y].cube);

                        // Reset the cell state
                        Cell cell = board[z].cells[x, y];
                        cell.state = CellState.Empty;
                        cell.cube = null;
                        board[z].cells[x, y] = cell;
                    }
                }
            }
        }

        // Ensure we have new random colors for each layer
        colors.Clear();
        for (int i = 0; i < roomRenderer.sizeZ; i++)
        {
            colors.Add(new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f)));
        }

        Debug.Log("Board has been reset");
    }

    public List<Layer> GetBoard()
    {
        return board;
    }

}
