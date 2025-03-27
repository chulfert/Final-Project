using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class BoardStateTests
{
    private GameObject boardGameObject;
    private BoardState boardState;
    private RoomRenderer roomRenderer;

    [SetUp]
    public void Setup()
    {
        boardGameObject = new GameObject();
        boardState = boardGameObject.AddComponent<BoardState>();
        roomRenderer = boardGameObject.AddComponent<RoomRenderer>(); 
        boardState.basicCube = new GameObject();
        boardState.current_polynomino = boardGameObject.AddComponent<Polynomino4D>();
        boardState.roomRenderer = roomRenderer;
        boardState.InitializeBoard();
    }

    [TearDown]
    public void TearDown()
    {
        
    }

    [Test]
    public void TestWorldToBoardTransfrom()
    {
        for(float z = 0; z < 10; z += 0.5f)
            for(float y =  -3; y <3; y+= 0.5f)
                for(float x = -3; x < 3; x += 0.5f)
                {
                    Vector3 worldPos = new Vector3(x, y, z);
                    Vector3Int boardPos = boardState.WorldToBoardIndex(worldPos);

                    Vector3 worldPos2 = boardState.BoardIndexToWorld(boardPos);

                    worldPos.x = Mathf.Ceil(worldPos.x);
                    worldPos.y = Mathf.Ceil(worldPos.y);
                    worldPos.z = Mathf.Ceil(worldPos.z);


                    worldPos2.x = Mathf.Ceil(worldPos2.x);
                    worldPos2.y = Mathf.Ceil(worldPos2.y);
                    worldPos2.z = Mathf.Ceil(worldPos2.z);
                    Assert.AreEqual(worldPos, worldPos2);
                }

    }

    public void TestCheckValidBoardPosition()
    {
        Vector3Int testVector = new Vector3Int(-1, 0, 0);
        bool negative = boardState.CheckValidBoardPosition(testVector);
        Assert.IsFalse(negative);
        testVector = new Vector3Int(0, 0, -5);
        negative = boardState.CheckValidBoardPosition(testVector);
        Assert.IsFalse(negative);
        bool positive = boardState.CheckValidBoardPosition(new Vector3Int(0, 0, 0));
        Assert.IsTrue(positive);
    }

    [Test]
    public void TestResetVector()
    {
        Vector3 testVector = new Vector3(1, 1, 1);
        Vector3 resetVector = boardState.ResetVector(testVector);
        Assert.AreEqual(new Vector3(0, 0, 0), resetVector);

        testVector = new Vector3(-5, 0, 0);
        resetVector = boardState.ResetVector(testVector);
        Assert.AreEqual(new Vector3(2, 0, 0), resetVector);

    }

    [Test]
    public void TestClearLayer()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                BoardState.Cell cell = new BoardState.Cell();
                cell.state = BoardState.CellState.Filled;
                boardState.board[9].cells[i,j] = cell;
            }
        }
        boardState.ClearLayer(9);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                BoardState.Cell cell = boardState.board[9].cells[i, j];    
                Assert.AreEqual(BoardState.CellState.Empty, cell.state);
            }
        }
    }

    [Test]
    public void TestCheckLayersForFull()
    {
        for (int i = 0; i < roomRenderer.sizeX; i++)
        {
            for (int j = 0; j < roomRenderer.sizeY; j++)
            {
                BoardState.Cell cell = new BoardState.Cell();
                cell.state = BoardState.CellState.Filled;
                boardState.board[9].cells[i, j] = cell;
            }
        }
        boardState.CheckLayersForFull();
        for (int i = 0; i < roomRenderer.sizeX; i++)
        {
            for (int j = 0; j < roomRenderer.sizeY; j++)
            {
                BoardState.Cell cell = boardState.board[9].cells[i, j];
                Assert.AreEqual(BoardState.CellState.Empty, cell.state);
            }
        }
    }

    public void TestClearFalling()
    {
        BoardState.Cell cell = new BoardState.Cell();
        cell.state = BoardState.CellState.Falling;

        boardState.board[9].cells[3, 2] = cell;

        boardState.ClearFalling();

        cell = boardState.board[9].cells[3, 2];
        Assert.AreEqual(BoardState.CellState.Empty, cell.state);
    }

    [Test]
    public void TestSetFalling()
    {
        boardState.SetFalling(new Vector3Int(2, 2, 9));
        int x = Mathf.CeilToInt(2 + roomRenderer.sizeX / 2);
        int y = Mathf.CeilToInt(2 + roomRenderer.sizeY / 2);
        BoardState.Cell cell = boardState.board[8].cells[x,y ];
        Assert.AreEqual(BoardState.CellState.Falling, cell.state);
    }

    [Test]
    public void TestGetFallingCubes()
    {
        List<Vector3Int> fallingCubes = boardState.GetFallingCubes();
        Assert.AreEqual(0, fallingCubes.Count);
        boardState.SetFalling(new Vector3Int(2, 2, 9));
        fallingCubes = boardState.GetFallingCubes();
        Assert.AreEqual(1, fallingCubes.Count);
    }
}
