using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityEditor.PlayerSettings;

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
        public Vector3 position;
        public GameObject cube;
    }
    private struct Layer
    {
        public Cell[,] cells;
    }

    private List<Layer> board;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomRenderer = GetComponent<RoomRenderer>();
        // Initialize the board state
        board = new List<Layer>();
        // get board heigh from roomRenderer
        int board_height = roomRenderer.sizeZ;
        // get board width from roomRenderer
        int board_x = roomRenderer.sizeX;
        // get board depth from roomRenderer
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
                    layer.cells[j, k].position = new Vector3(j, i, k);
                    layer.cells[j, k].cube = null;
                }
            }
            board.Add(layer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // From a given position, check if this is available
    public bool CheckNextFree(Vector3 targetPosition)
    {
        int x = (int)Math.Round(targetPosition.x) + (int)(GetBoardExtends().x / 2);
        int y = (int)Math.Round(targetPosition.y) + (int)(GetBoardExtends().y / 2);
        int z = (int)Math.Floor(targetPosition.z);
        if (board[z].cells[x, y].state == CellState.Filled)
        {
            return false;
        }
        return true;
    }

    public bool CheckBounds(Vector3 target)
    {
        if (target.x < GetBoardOrigin().x) return false;
        if (target.y < GetBoardOrigin().y) return false;
        if(target.x > GetBoardOrigin().x + GetBoardExtends().x * current_polynomino.cubeSize) return false;
        if (target.y > GetBoardOrigin().y + GetBoardExtends().y * current_polynomino.cubeSize) return false;
        return true;
    }

    public bool CheckZBounds(Vector3 target)
    {
        if(Mathf.Floor(target.z) >  GetBoardExtends().z * current_polynomino.cubeSize - 1) return false;
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
            Vector3 pos = cube.GetPosition3D() + polynomino.transform.position;
            int x = (int)Math.Round(pos.x) + (int)(GetBoardExtends().x / 2);
            int y = (int)Math.Round(pos.y) + (int)(GetBoardExtends().y / 2);
            int z = (int)Math.Floor(pos.z);
            board[z].cells[x, y].state = CellState.Filled;

            // Create a cube from the basic cube prefab and set it to this position
            Vector3 pos3 = new Vector3(x - (int)(GetBoardExtends().x / 2), y - (int)(GetBoardExtends().x / 2), z + 1);
            GameObject go = Instantiate(basicCube, pos3, Quaternion.identity);
            go.transform.localScale = new Vector3(polynomino.cubeSize, polynomino.cubeSize, polynomino.cubeSize);
            board[z].cells[x, y].cube = go;
            // Assign color based on z heioght
            float color = (float)z / (float)roomRenderer.sizeZ;
            go.GetComponent<MeshRenderer>().material.color = new Color(color, color, color);

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
                        board[i].cells[j, k].cube.transform.position = board[i].cells[j, k].position;
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
}
