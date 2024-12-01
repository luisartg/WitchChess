using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* IDs
 * 0 = empty space
 * 1 = king
 * 2 = go
 */



public class GameManager : MonoBehaviour
{
    public int playerMovementCounter = 2;
    public bool doSwap = false;

    public List<GameObject> basicGamePieces = new List<GameObject>();
    public string[,] boardSpaces = new string[5,5];
    
    [TextArea]
    public string gameConfig = "";

    private BoardManager boardManager;

    private void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        GenerateGame();
    }

    public void GenerateGame()
    {
        int rowIndex = 0;
        string[] gameRows = gameConfig.Split("\n");
        foreach (string row in gameRows) 
        {
            string[] gameColumns = row.Split(",");
            for (int i = 0; i < gameColumns.Length; i++)
            {
                boardSpaces[rowIndex,i] = gameColumns[i];
            }
            rowIndex++;
        }
        Debug.Log($"BoardConfig = {boardSpaces}");
        boardManager.AddPiecesToBoard(boardSpaces, basicGamePieces);
    }

    public void MovePlayerPieceTo(Vector2 newPosition)
    {
        if (playerMovementCounter > 0)
        {
            boardManager.MovePlayerTo(newPosition, doSwap);
            playerMovementCounter--;
        }
    }

    public void AddPlayerMoveCounter()
    {
        playerMovementCounter++;
    }

    public void PrepareForPlayerSwap()
    {
        doSwap = true;
    }
    
}

public class PieceData
{
}
