using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public TextMeshProUGUI dialogText;
    private GameManager gameManager;
    public GameObject dialogContainer;

    private int currentLineIndex = 0;
    private string[] currentLines;
    private bool startDisplay = false;

    public delegate void ContinueAfterDialog();

    ContinueAfterDialog currentCallback;


    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (startDisplay)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DisplayLine();
            }
            
        }
    }

    public void ShowDialog(string text, ContinueAfterDialog callback)
    {
        currentCallback = callback;//we save this to callback when finishin displaying data

        currentLines = text.Split("\n");
        currentLineIndex = 0;
        startDisplay = true;

        DisplayLine();
    }

    private void DisplayLine()
    {
        if (currentLineIndex < currentLines.Length)
        {
            dialogText.text = currentLines[currentLineIndex];
            currentLineIndex++;
            dialogContainer.SetActive(true);
        }
        else
        {
            dialogContainer.SetActive(false);
            startDisplay = false;
            currentCallback();
            //gameManager.StartCurrentGame();
            //tell game manager to continue game
        }
    }
}
