using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        // 01
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "Alright kitty kat! You are going to be my best friend. Let's play a game!" +
            "\nRemember, you are the Red King. You can only move once." +
            "\nI will be the other pieces.\n" +
            "[Go] pieces will eat you if they enclose the King (north/south/east/west)" +
            "\n Try to escape. Let's play!";
        sessionData.GameConfig = "2,2,2,2,2\n2,2,2,2,2\n2,2,1,2,2\n2,0,2,0,2\n2,2,2,2,2";
        sessionData.LoseText = "Ah! ... Too bad... for you of course.";
        sessionData.WinText = "Hum! We'll see in the next one.";
        sessions.Add(sessionData);

        // 02
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "This time I'm using Rooks; its arrows will attack you horizontally and vertically..." +
            "\nBut they won't go over other pieces." +
            "\n Let's play.";
        sessionData.GameConfig = "0,2,2,0,0\n0,2,2,0,0\n1,2,2,0,0\n0,2,2,0,0\n3,0,0,0,0";
        sessionData.LoseText = "What fun! Let's go again!";
        sessionData.WinText = "O.K., O.K., next!!";
        sessions.Add(sessionData);

        // 03
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "What is this?\n A cookie?!!... errr... alright!" +
            "\nIf you eat it, your King will be able to move once more." +
            "\nLet's play!";
        sessionData.GameConfig = "2,2,1,2,2\n2,2,5,2,2\n0,0,2,0,0\n0,0,0,0,0\n0,3,0,3,0";
        sessionData.LoseText = "Ha ha ha! I win, I win. Wanna go again?";
        sessionData.WinText = "It was the cookie's fault! Hum!";
        sessions.Add(sessionData);

        // 04
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "What is this thing?" +
            "\nAh, whatever... It allows you to ..aahh.. swap places with other near piece! Yes!" +
            "\nLet's play.";
        sessionData.GameConfig = "0,0,2,0,0\n2,2,6,2,2\n0,0,1,0,0\n0,0,0,0,0\n0,3,3,3,0";
        sessionData.LoseText = "Ah! ... Too bad... for you of course hehe. Let's go again!";
        sessionData.WinText = "Alright, this is the one!";
        sessions.Add(sessionData);

        // 05
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "Aha!" +
            "\nThe secretive Bishop. Always lurking around the corners." +
            "\nBe careful because it attacks you diagonally." +
            "\nLet's play!";
        sessionData.GameConfig = "0,0,0,3,0\n4,0,0,0,3\n0,0,1,0,0\n3,0,6,0,4\n0,3,2,0,0";
        sessionData.LoseText = "Am I good or what! Ha ha!";
        sessionData.WinText = "Dang it!";
        sessions.Add(sessionData);
                
        // 06
        sessionData = new GameSessionData();
        sessionData.DialogText =
            "I present to you my impenetrable fortress!!" +
            "\nLet's play.";
        sessionData.GameConfig = "0,0,3,0,0\n2,6,2,0,2\n3,2,1,2,3\n2,0,2,0,2\n0,0,3,0,0";
        sessionData.LoseText = "Ha! No one expects my fortress! I'm invincible! I'll let you try again!";
        sessionData.WinText = "What? Nooo! My fortress!" +
            "\n..." +
            "\nSo the Red King scapes... for not for long  HE HE HE!";
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
        if (currentGameSession >= sessions.Count)
        {
            SceneManager.LoadScene("Ending");
        }
        else
        {
            SetCameraState(CameraState.GeneralView);
            dialogManager.ShowDialog(sessions[currentGameSession].DialogText, StartCurrentGame);
        }
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
