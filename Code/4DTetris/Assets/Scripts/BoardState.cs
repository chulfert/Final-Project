using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class BoardState : MonoBehaviour
{
    // References
    [Header("References")]
    public RoomRenderer roomRenderer;
    public Polynomino4D current_polynomino;
   

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
        public CubeRep cube;
    }

    private struct Layer
    {
        public Cell[,] cells;
    }

    private List<Layer> board;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        int x = (int)targetPosition.x;
        int y = (int)targetPosition.y;
        int z = (int)targetPosition.z;
        if (board[y].cells[x, z].state == CellState.Empty)
        {
            return true;
        }
        return false;
    }

    public void TransferCubes(Polynomino4D polynomino)
    {
        // Take the cubes from the polynomino and put them in the board, then destroy the polyomino
        for (int i = 0; i < polynomino.cubes.Count; i++)
        {
            Vector3 position = polynomino.cubes[i].transform.position;
            int x = (int)position.x;
            int y = (int)position.y;
            int z = (int)position.z;
            board[z].cells[x, y].state = CellState.Filled;
            board[z].cells[x, y].cube = polynomino.cubes[i];
        }
        Destroy(polynomino.gameObject);
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
