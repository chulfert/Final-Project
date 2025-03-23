using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.Rendering.VolumeComponent;
using static Polynomino4D;
using TMPro;

public class BoardState : MonoBehaviour
{
    // References
    [Header("References")]
    private RoomRenderer roomRenderer;
    public Polynomino4D current_polynomino;
    public GameObject basicCube;

    // States of each cell
    public enum CellState
    {
        Empty,
        Filled,
        Falling,
    }

    private struct Cell
    {
        public CellState state;
        public GameObject cube;
    }
    private struct Layer
    {
        public Cell[,] cells;
    }

    private List<Layer> board;

    List<Color> colors = new List<Color>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
            colors.Add(new Color(UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f), UnityEngine.Random.Range(0.0f, 1.0f)));
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckLayersForFull();
        //string debug = "";
        // Debug the board state
        //for (int i = 0; i < board.Count; i++)
        //{
        //    debug += "Layer: " + i + ": ";
        //    for (int j = 0; j < board[i].cells.GetLength(0); j++)
        //    {
        //        for (int k = 0; k < board[i].cells.GetLength(1); k++)
        //        {
        //            debug += "X: " + j + "Y: " + board[i].cells[j, k].state + "\n";
        //        }
        //    }
        //}
        //Debug.Log(debug);
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
        /*if (target.x < GetBoardOrigin().x) return false;
        if (target.y < GetBoardOrigin().y) return false;
        if(target.x > GetBoardOrigin().x + GetBoardExtends().x * current_polynomino.cubeSize) return false;
        if (target.y > GetBoardOrigin().y + GetBoardExtends().y * current_polynomino.cubeSize) return false;*/
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

    //TODO: THIS IS HORRIBLE COUPLING!!!! CHANGE IT!!!!
    public Vector3 GetBoardOrigin()
    {
        // find GO Board
        GameObject go = GameObject.Find("Board");
        // get the size of the board    
        return go.transform.position;
    }

    //TODO: THIS IS HORRIBLE COUPLING!!!! CHANGE IT!!!!
    public Vector3 GetBoardExtends()
    {
        // finf GO Board and RoomRenderer component
        GameObject go = GameObject.Find("Board");
        RoomRenderer rr = go.GetComponent<RoomRenderer>();
        // get the size of the board
        return new Vector3(rr.sizeX, rr.sizeY, rr.sizeZ);
    }

    public void TransferCubes(Polynomino4D polynomino)
    {
        // Check the hypercubes of the polynominoe, calculate the position in the board grid and set it to filled
        foreach (var cube in polynomino.hypercubes)
        {
            if (!cube.IsVisible()) continue;
            Vector3 pos = cube.GetPosition3D() + polynomino.targetPosition;
            Vector3Int index = WorldToBoardIndex(pos);
            if (!CheckValidBoardPosition(index))
            {
                GameObject.Find("GameManager").GetComponent<GameStateManager>().GameOver();
            }
            board[index.z].cells[index.x, index.y].state = CellState.Filled;

            // Create a cube from the basic cube prefab and set it to this position
            Vector3 pos3 = BoardIndexToWorld(index);
            //pos3.y += 1;
            //pos3.z -= 0.5f;

            GameObject go = Instantiate(basicCube, pos3, Quaternion.identity);
            go.transform.localScale = new Vector3(polynomino.cubeSize, polynomino.cubeSize, polynomino.cubeSize);
            board[index.z].cells[index.x, index.y].cube = go;

            // Assign color based on z heioght
            //float color = (float)z / (float)roomRenderer.sizeZ;
            Color c = colors[index.z];
            go.GetComponent<MeshRenderer>().material.color = c;

        }
        Destroy(polynomino.gameObject);
        GameObject.Find("GameManager").GetComponent<PolyManager>().SpawnNewPolynomino();
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
                ClearLayer(i);
            }
        }

    }

    public void ClearLayer(int layer)
    {
        // destroy the full layer and move everything at a lower Z towards z by 1, spawn a new layer at the top
        for (int i = 0; i < board[layer].cells.GetLength(0); i++)
        {
            for (int j = 0; j < board[layer].cells.GetLength(1); j++)
            {
                Destroy(board[layer].cells[i, j].cube.gameObject);
                board[layer].cells[i, j].state = CellState.Empty;
                board[layer].cells[i, j].cube = null;
            }
        }
        for (int i = layer; i < board.Count - 1; i++)
        {
            for (int j = 0; j < board[i].cells.GetLength(0); j++)
            {
                for (int k = 0; k < board[i].cells.GetLength(1); k++)
                {
                    board[i].cells[j, k].state = board[i + 1].cells[j, k].state;
                    board[i].cells[j, k].cube = board[i + 1].cells[j, k].cube;
                    if (board[i].cells[j, k].cube != null)
                    {
                        Vector3 pos = board[i].cells[j, k].cube.transform.position;
                        pos.z += 1;
                        board[i].cells[j, k].cube.transform.position = pos;
                    }
                }
            }
        }
        for (int i = 0; i < board[board.Count - 1].cells.GetLength(0); i++)
        {
            for (int j = 0; j < board[board.Count - 1].cells.GetLength(1); j++)
            {
                board[board.Count - 1].cells[i, j].state = CellState.Empty;
                board[board.Count - 1].cells[i, j].cube = null;
            }
        }


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

    private Vector3Int WorldToBoardIndex(Vector3 pos)
    {
        /*Vector3 ext = GetBoardExtends();
        int x = Mathf.FloorToInt(pos.x) + Mathf.FloorToInt(ext.x / 2f);
        int y = Mathf.FloorToInt(pos.y) + Mathf.FloorToInt(ext.y / 2f);
        int z = Mathf.FloorToInt(pos.z) - 1;  // assuming z remains as is.
        return new Vector3Int(x, y, z);*/


        Vector3 boardOrigin = GetBoardOrigin();
        // Subtract the board origin and divide by cube size (assume uniform scaling)
        int x = Mathf.RoundToInt((pos.x - boardOrigin.x) / current_polynomino.cubeSize) - 1;
        int y = Mathf.RoundToInt((pos.y - boardOrigin.y) / current_polynomino.cubeSize) - 1;
        int z = Mathf.RoundToInt((pos.z - boardOrigin.z) / current_polynomino.cubeSize) - 1;
        return new Vector3Int(x, y, z);
    }

    private Vector3 BoardIndexToWorld(Vector3Int index)
    {
        /*Vector3 ext = GetBoardExtends();
        float x = (index.x - ext.x / 2f);
        float y = (index.y - ext.y / 2f);
        float z = index.z + 1;
        return new Vector3(x, y, z);*/


        Vector3 boardOrigin = GetBoardOrigin();
        return new Vector3(index.x * current_polynomino.cubeSize + boardOrigin.x + 1, index.y * current_polynomino.cubeSize + boardOrigin.y + 1, index.z * current_polynomino.cubeSize + boardOrigin.z + 1);

    }

    private bool CheckValidBoardPosition(Vector3Int index)
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
        if (index.z < 0)
        {
            mod.z = -index.z;
        }
        if (index.z >= GetBoardExtends().z)
        {
            mod.z = GetBoardExtends().z - index.z - 1;
        }
        return mod;


    }
}
