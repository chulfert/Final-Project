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
        for(float z = 0; z < 20; z += 0.1f)
            for(float y =  -3; y <3; y+= 0.1f)
                for(float x = -3; x < 3; x += 0.1f)
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
}
