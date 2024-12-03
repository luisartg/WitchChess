using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardManager : MonoBehaviour
{
    public Vector2 BoardSize = new(5,5);
    public float pieceMovementSpeed = 4; //units per second

    private List<GamePieceData> piecesOnBoard = new List<GamePieceData>();
    private List<GameObject> currentPiecesList;

    private MouseEffects mouseEffects;

    private GameManager gameManager;

    public delegate void CallbackAfterMatchEnd(WinLoseResult result);

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        mouseEffects = FindFirstObjectByType<MouseEffects>();
    }

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

                CreatePieceOnBoardAt(j, i, positionId);
            }
        }
        mouseEffects.ForcedUpdatePlayerPos();
    }

    private void CreatePieceOnBoardAt(int x, int y, string pieceId)
    {
        GameObject pieceReference = GetPieceById(int.Parse(pieceId));
        if (pieceReference != null)
        {
            GameObject newPiece = Instantiate(pieceReference,
                new Vector3(x, 0, y),
                Quaternion.identity);
            GamePieceData newPieceData = new GamePieceData();
            newPieceData.PieceObject = newPiece;
            newPieceData.boardPosition = new Vector2(x, y);
            piecesOnBoard.Add(newPieceData);
            newPiece.transform.parent = this.transform;

            newPiece.GetComponent<BasicGamePiece>().DoIntroduction();
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
            if (doSwap)
            {
                //go to king's place
                pieceData.PieceObject.transform.position = 
                    new Vector3(
                        piecesOnBoard[playerPieceIndex].boardPosition.x,
                        pieceData.PieceObject.transform.position.y,
                        piecesOnBoard[playerPieceIndex].boardPosition.y);
                pieceData.boardPosition = piecesOnBoard[playerPieceIndex].boardPosition;
                gameManager.ConsumeSwapEffect();
            }
            else 
            {
                pieceData.PieceObject.GetComponent<BasicGamePiece>().RemoveThisPiece();
            }

        }

        // Move player piece to new position
        piecesOnBoard[playerPieceIndex].PieceObject.transform.position 
            = new Vector3(newPosition.x,
                          piecesOnBoard[playerPieceIndex].PieceObject.transform.position.y,
                          newPosition.y);
        piecesOnBoard[playerPieceIndex].boardPosition = newPosition;

        //Remove unrequired piece from list
        if (targetPieceIndex >= 0 && !doSwap)
        {
            piecesOnBoard.RemoveAt(targetPieceIndex);
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

    public Vector2 GetCurrentPlayerPosition()
    {
        int index = FindPlayerPiece();
        return piecesOnBoard[index].boardPosition;
    }

    public void ReviewWinLose(CallbackAfterMatchEnd callbackAfterMatchEnd)
    {
        WinLoseResult win;
        //create position map
        BoardMapData boardMapData = CreateBoardMap();

        // Check if go pieces enclose the king
        win = CheckForGoLose(boardMapData);
        if (!win.success)
        {
            callbackAfterMatchEnd(win);
            return;
        }

        //Check for Bishop
        win = CheckForBishopLose(boardMapData);
        if (!win.success)
        {
            EatKingByPieceLocatedAt(win.enemyPiecePosition);
            callbackAfterMatchEnd(win);
            return;
        }

        //Check for Rook
        win = CheckForRookLose(boardMapData);
        if (!win.success)
        {
            EatKingByPieceLocatedAt(win.enemyPiecePosition);
            callbackAfterMatchEnd(win);
            return;
        }

        callbackAfterMatchEnd(win);
    }

    private WinLoseResult CheckForRookLose(BoardMapData boardMapData)
    {
        // ID = 3
        WinLoseResult win = new WinLoseResult();
        //go up
        int x, y;
        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;

        y++;
        while (y <= 4)
        {
            if (boardMapData.Map[x, y] == 0)
            {
                y++;
                continue; // empty space, continue search
            }
            if (boardMapData.Map[x, y] != 3) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 3)
            {

                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "Ha! Rook takes the King! Yay!";
                return win;
            }
            y++;
        }

        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        y--;
        while (y >=0)
        {
            if (boardMapData.Map[x, y] == 0) { y--; continue; } // empty space, continue search
            if (boardMapData.Map[x, y] != 3) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 3)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "Ha! Rook takes the King! Yay!";
                return win;
            }
            y--;
        }

        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        x++;
        while (x <= 4)
        {

            if (boardMapData.Map[x, y] == 0) { x++; continue; } // empty space, continue search
            if (boardMapData.Map[x, y] != 3) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 3)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "Ha! Rook takes the King! Yay!";
                return win;
            }
            x++;
        }

        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        x--;
        while (x >= 0)
        {

            if (boardMapData.Map[x, y] == 0) {x--; continue;} // empty space, continue search
            if (boardMapData.Map[x, y] != 3) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 3)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "Ha! Rook takes the King! Yay!";
                return win;
            }
            x--;
        }

        win.success = true;
        win.message = "Mmm you win...";

        return win;
    }

    private WinLoseResult CheckForBishopLose(BoardMapData boardMapData)
    {
        // ID = 3
        WinLoseResult win = new WinLoseResult();
        //go up
        int x, y;
        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        
        //go uphill right
        y++;
        x++;
        while (y <= 4 && x <= 4)
        {
            if (boardMapData.Map[x, y] == 0)
            {
                y++;
                x++;
                continue;// empty space, continue search
            }
            if (boardMapData.Map[x, y] != 4) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 4)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "He he he! The Bishop stabs you!";
                return win;
            }
            y++;
            x++;
        }

        //go downhill left
        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        y--;
        x--;
        while (y >= 0 && x >= 0)
        {
            if (boardMapData.Map[x, y] == 0) 
            {
                y--;
                x--; 
                continue; 
            }// empty space, continue search
            if (boardMapData.Map[x, y] != 4) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 4)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "He he he! The Bishop stabs you!";
                return win;
            }
            y--;
            x--;
        }

        //go uphill left
        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        y++;
        x--;
        while (y <= 4 && x >= 0)
        {
            if (boardMapData.Map[x, y] == 0) 
            {
                y++;
                x--; 
                continue; 
            } // empty space, continue search
            if (boardMapData.Map[x, y] != 4) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 4)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "He he he! The Bishop stabs you!";
                return win;
            }
            y++;
            x--;
        }

        //go downhill right
        x = (int)boardMapData.kingPos.x;
        y = (int)boardMapData.kingPos.y;
        y--;
        x++;
        while (y >= 0 && x <= 4)
        {
            if (boardMapData.Map[x, y] == 0)
            {
                y--;
                x++;
                continue;
            } // empty space, continue search
            if (boardMapData.Map[x, y] != 4) break; // not a rook, interrupt search
            if (boardMapData.Map[x, y] == 4)
            {
                win.enemyPiecePosition = new Vector2(x, y);
                win.success = false;
                win.message = "He he he! The Bishop stabs you!";
                return win;
            }
            y--;
            x++;
        }

        win.success = true;
        win.message = "Mmm you win...";

        return win;
    }

    private BoardMapData CreateBoardMap()
    {
        BoardMapData boardMapData = new BoardMapData();

        boardMapData.Map = new int[5, 5];

        foreach (var pieceData in piecesOnBoard)
        {
            int id = pieceData.PieceObject.GetComponent<BasicGamePiece>().PieceID;
            boardMapData.Map[
                (int)pieceData.boardPosition.x,
                (int)pieceData.boardPosition.y
                ] = id;

            if (id == 1)
            {
                boardMapData.kingPos = pieceData.boardPosition;
            }
        }
        return boardMapData;
    }

    private WinLoseResult CheckForGoLose(BoardMapData data)
    {
        WinLoseResult winLoseResult = new WinLoseResult();
        int x, y;
        int reviewId;
        int sx = 0;
        int sy = 0;
        int spacesCount = 0;
        int goCount = 0;
        //top
        x = (int)data.kingPos.x;
        y = (int)data.kingPos.y+1;
        if (y <= 4)
        {
            reviewId = data.Map[x, y];
            if (reviewId == 2)// is a Go piece
            {
                goCount++;
            }
            else if (reviewId == 0)
            { 
                spacesCount++;
                sx = x;
                sy = y;
            }
        }
        else
        {
            //out of bounds counts as Go piece
            goCount++;
        }

        //bottom
        x = (int)data.kingPos.x;
        y = (int)data.kingPos.y-1;
        if (y >= 0)
        {
            reviewId = data.Map[x, y];
            if (reviewId == 2)// is a Go piece
            {
                goCount++;
            }
            else if (reviewId == 0)
            {
                spacesCount++;
                sx = x;
                sy = y;
            }
        }
        else
        {
            //out of bounds counts as Go piece
            goCount++;
        }
        //left
        x = (int)data.kingPos.x-1;
        y = (int)data.kingPos.y;
        if (x >= 0)
        {
            reviewId = data.Map[x, y];
            if (reviewId == 2)// is a Go piece
            {
                goCount++;
            }
            else if (reviewId == 0)
            {
                spacesCount++;
                sx = x;
                sy = y;
            }
        }
        else
        {
            //out of bounds counts as Go piece
            goCount++;
        }

        //right
        x = (int)data.kingPos.x+1;
        y = (int)data.kingPos.y;
        if (x <= 4)
        {
            reviewId = data.Map[x, y];
            if (reviewId == 2)// is a Go piece
            {
                goCount++;
            }
            else if (reviewId == 0)
            {
                spacesCount++;
                sx = x;
                sy = y;
            }
        }
        else
        {
            //out of bounds counts as Go piece
            goCount++;
        }

        if (goCount == 4)
        {
            winLoseResult.success = false;
            winLoseResult.message = "Ah... your king drowned between Go pieces... So sad!";
            return winLoseResult; //King drowned
        }
        if (goCount == 3 && spacesCount == 1)
        {
            //place go piece at sx,sy
            CreatePieceOnBoardAt(sx, sy, "2");// 2 is ID for Go piece

            winLoseResult.success = false;
            winLoseResult.message = "Death by Go! He he he!";
            return winLoseResult; // king lost
        }

        winLoseResult.success = true;
        winLoseResult.message = "Mmm you win...";
        return winLoseResult;
    }

    private void EatKingByPieceLocatedAt(Vector2 enemyPos)
    {
        int playerPieceIndex = FindPlayerPiece();
        int targetPieceIndex = FindPieceLocatedAt(enemyPos);

        GamePieceData enemyPiece = piecesOnBoard[targetPieceIndex];
        GamePieceData playerPiece = piecesOnBoard[playerPieceIndex];

        StartCoroutine(MovePieceToPlayer(enemyPiece, playerPiece));
        //move piece from start position to kings place
        //when piece reaches the king, remove king

        

    }

    private IEnumerator MovePieceToPlayer(GamePieceData enemyPiece, GamePieceData playerPiece)
    {
        Vector2 direction = GetDirectionFromAtoB(enemyPiece.boardPosition, playerPiece.boardPosition);
        direction = direction.normalized;
        Vector2 startDirection = direction;

        while (IsDirectionSimilar(direction, startDirection))
        {
            Vector3 nextPosition = enemyPiece.PieceObject.transform.position;
            nextPosition.x += startDirection.x * pieceMovementSpeed * Time.deltaTime;
            nextPosition.z += startDirection.y * pieceMovementSpeed * Time.deltaTime;
            enemyPiece.PieceObject.transform.position = nextPosition;

            yield return new WaitForEndOfFrame();
            direction = GetDirectionFromAtoB(
                new Vector2(
                    nextPosition.x, 
                    nextPosition.z), 
                playerPiece.boardPosition).normalized;
        }

        //enemy piece reached, remove king
        if (playerPiece != null)
        {
            playerPiece.PieceObject.GetComponent<BasicGamePiece>().RemoveThisPiece();
        }
    }

    private Vector2 GetDirectionFromAtoB(Vector2 startPosition, Vector2 endPosition)
    {
        return new Vector2(
            endPosition.x - startPosition.x,
            endPosition.y - startPosition.y);
    }

    private bool IsDirectionSimilar(Vector2 nv1, Vector2 nv2, float degreeMargin = 1)
    {
        float alphaDegrees = GetArcInDegrees(nv1);
        float betaDegrees = GetArcInDegrees(nv2);
        alphaDegrees += 360;
        betaDegrees += 360;
        if (alphaDegrees < betaDegrees + degreeMargin && alphaDegrees > betaDegrees - degreeMargin)
        {
            //both directions are the same inside the defined range
            return true;
        }
        else
        {
            return false;
        }
    }

    private float GetArcInDegrees(Vector2 direction)
    {
        float xDegrees = Mathf.Rad2Deg * Mathf.Asin(direction.x);
        if (direction.y < 0)
        {
            xDegrees = 360 - xDegrees;
        }
        return xDegrees;
    }

}

public class GamePieceData
{
    public GameObject PieceObject;
    public Vector2 boardPosition;
}

public class BoardMapData
{
    public int[,] Map;
    public Vector2 kingPos;
}

public class WinLoseResult
{
    public bool success;
    public string message;
    public Vector2 enemyPiecePosition;
}
