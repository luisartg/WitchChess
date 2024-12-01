using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Vector2 BoardSize = new(5,5);

    private List<GamePieceData> piecesOnBoard = new List<GamePieceData>();
    private List<GameObject> currentPiecesList;

    private GameObject GetPieceById(int id)
    {
        foreach (GameObject piece in currentPiecesList)
        {
            if (piece.GetComponent<BasicGamePiece>().PieceID == id)
            {
                return piece;
            }
        }
        return null;
    }

    private void ClearCurrentPieces()
    {
        foreach (var piece in piecesOnBoard)
        {
            Destroy(piece.PieceObject.gameObject);
        }
        piecesOnBoard.Clear();
    }

    public void AddPiecesToBoard(string[,] positionMap, List<GameObject> piecesList)
    {
        currentPiecesList = piecesList;
        ClearCurrentPieces();
        //rows
        for (int i = 0; i < positionMap.GetLength(0); i++)
        {
            //columns
            for (int j = 0; j < positionMap.GetLength(1); j++)
            {
                string positionId = positionMap[i, j];
                if (positionId == null) continue; //this is for empty spaces in the defined board

                GameObject pieceReference = GetPieceById(int.Parse(positionId));
                if (pieceReference != null)
                {
                    GameObject newPiece = Instantiate(pieceReference,
                        new Vector3(j, 0, i),
                        Quaternion.identity);
                    GamePieceData newPieceData = new GamePieceData();
                    newPieceData.PieceObject = newPiece;
                    newPieceData.boardPosition = new Vector2(j, i);
                    piecesOnBoard.Add(newPieceData);
                    newPiece.transform.parent = this.transform;
                }
            }
        }
    }

    private int FindPieceLocatedAt(Vector2 newPosition)
    {
        for (int i=0;i<piecesOnBoard.Count;i++) 
        {
            if (newPosition.x == piecesOnBoard[i].boardPosition.x 
                && newPosition.y == piecesOnBoard[i].boardPosition.y)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindPlayerPiece()
    {
        for (int i = 0; i < piecesOnBoard.Count; i++)
        {
            if (piecesOnBoard[i].PieceObject.GetComponent<BasicGamePiece>().PieceID == 1)
            {
                return i;
            }
        }
        return -1;
    }

    public void MovePlayerTo(Vector2 newPosition, bool doSwap)
    {
        int playerPieceIndex = FindPlayerPiece();
        int targetPieceIndex = FindPieceLocatedAt(newPosition);

        // If anyone is in the same place, then remove piece or swap
        if (targetPieceIndex >= 0)
        {
            GamePieceData pieceData = piecesOnBoard[targetPieceIndex];
            pieceData.PieceObject.transform.parent = null;
            Destroy(pieceData.PieceObject.gameObject);
        }

        // Move player piece to new position
        piecesOnBoard[playerPieceIndex].PieceObject.transform.position 
            = new Vector3(newPosition.x,
                          piecesOnBoard[playerPieceIndex].PieceObject.transform.position.y,
                          newPosition.y);
        piecesOnBoard[playerPieceIndex].boardPosition = newPosition;

        //Remove unrequired piece from list
        if (targetPieceIndex >= 0)
        {
            piecesOnBoard.RemoveAt(targetPieceIndex);
            //TODO: CHECK FOR EFFECTS BEFORE REMOVING
            //AT THIS POINT ALL INDEXES ARE MOVED, WE HAVE TO SEARCH AGAIN
        }
    }

    /*
    private void RemovePieceFromBoardLocatedAt(Vector2 newPosition)
    {
        for (var i = 0; i <= piecesOnBoard.Count; i++)
        {
            Vector3 piecePos = piecesOnBoard[i].transform.position;
            if (piecePos.x == newPosition.x && piecePos.z == newPosition.y)
            {
                piecesOnBoard[i].gameObject.transform.parent = null;
                Destroy(piecesOnBoard[i].gameObject);
                piecesOnBoard.RemoveAt(i);
            }
        }
    }
    */
}

public class GamePieceData
{
    public GameObject PieceObject;
    public Vector2 boardPosition;
}
