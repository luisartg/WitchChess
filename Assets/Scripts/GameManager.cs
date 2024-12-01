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
    public int playerMovementCounter = 1;
    public bool doSwap = false;

    public List<GameObject> basicGamePieces = new List<GameObject>();
    public string[,] boardSpaces = new string[5,5];
    
    [TextArea]
    public string gameConfig = "";

    private List<GameSessionData> sessions = new List<GameSessionData>();

    private BoardManager boardManager;

    private MouseEffects mouseEffect;

    private int currentGameSession = -1;

    private DialogManager dialogManager;
    public Animator cameraAnimator;

    private bool isCameraCloseUp = false;

    private void Start()
    {
        mouseEffect = FindObjectOfType<MouseEffects>();
        mouseEffect.SetActiveMouseEffect(false);

        dialogManager = FindObjectOfType<DialogManager>();

        boardManager = FindObjectOfType<BoardManager>();
        CreateGameSessions();
        PrepareNextGame();
    }

    private void CreateGameSessions()
    {
        GameSessionData sessionData;
        
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "Let's see if you can win this." +
            "\nRemember, you are the Red King." +
            "\nI will be the other pieces.\n" +
            "Go pieces will eat you if they enclose the king (north/south/east/west)" +
            "\n Let's play.";
        sessionData.GameConfig = "2,2,2,2,2\n2,2,2,2,2\n2,2,1,2,2\n2,5,2,6,2\n2,2,2,2,2";
        sessionData.LoseText = "Ah! ... Too bad... for you of course.";
        sessionData.WinText = "Hum! We'll see in the next one.";
        sessions.Add(sessionData);

        sessionData = new GameSessionData();
        sessionData.DialogText =
            "Hi, this is a test 02" +
            "\n Let's play.";
        sessionData.GameConfig = "2,2,2,2,2\n2,2,2,2,2\n2,2,1,2,2\n2,0,2,0,2\n3,4,5,6,2";
        sessionData.LoseText = "Ah! ... Too bad... for you of course hehe.";
        sessionData.WinText = "Hum! We'll see in the next one, again.";
        sessions.Add(sessionData);
    }

    public void GenerateGame()
    {
        playerMovementCounter = 1;
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
        StartCoroutine(WaitforGamePiecesSet());
    }

    private IEnumerator WaitforGamePiecesSet()
    {
        yield return new WaitForSeconds(1);
        mouseEffect.SetActiveMouseEffect(true);
    }

    public void MovePlayerPieceTo(Vector2 newPosition)
    {
        if (playerMovementCounter > 0)
        {
            boardManager.MovePlayerTo(newPosition, doSwap);
            playerMovementCounter--;
        }

        if (playerMovementCounter <= 0)
        {
            CheckForWinLoseCondition();
        }
    }

    private void CheckForWinLoseCondition()
    {
        mouseEffect.SetActiveMouseEffect(false);

        WinLoseResult result = boardManager.ReviewWinLose();
        SetCameraState(CameraState.GeneralView);
        if (result.success)
        {
            //WIN SESSION
            dialogManager.ShowDialog(result.message + "\n" + sessions[currentGameSession].WinText, PrepareNextGame);
        }
        else
        {
            //LOST SESSION
            dialogManager.ShowDialog(result.message + "\n"+sessions[currentGameSession].LoseText, RepeatSession);
        }
    }

    private void RepeatSession()
    {
        currentGameSession--;
        PrepareNextGame();
    }

    private void PrepareNextGame()
    {
        currentGameSession++;
        SetCameraState(CameraState.GeneralView);
        dialogManager.ShowDialog(sessions[currentGameSession].DialogText, StartCurrentGame);
    }

    private void SetCameraState(CameraState state)
    {
        if (state == CameraState.GeneralView)
        {
            if (isCameraCloseUp)
            {
                cameraAnimator.SetTrigger("ChangeView");
                isCameraCloseUp = false;
            }
        }
        else if(state == CameraState.CloseUp)
        {
            if (!isCameraCloseUp)
            {
                cameraAnimator.SetTrigger("ChangeView");
                isCameraCloseUp = true;
            }
        }
    }

    public void StartCurrentGame()
    {
        SetCameraState(CameraState.CloseUp);
        gameConfig = sessions[currentGameSession].GameConfig;
        GenerateGame();
    }

    public void AddPlayerMoveCounter()
    {
        playerMovementCounter++;
    }

    public void PrepareForPlayerSwap()
    {
        doSwap = true;
    }

    public void ConsumeSwapEffect()
    {
        doSwap = false;
    }
    
}

public class GameSessionData
{
    public string GameConfig;
    public string DialogText;
    public string LoseText;
    public string WinText;
}

public enum CameraState
{
    CloseUp,
    GeneralView
}
