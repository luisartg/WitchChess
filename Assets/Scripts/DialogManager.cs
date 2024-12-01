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

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (startDisplay)
        {
            if (Input.GetMouseButtonUp(0))
            {
                DisplayLine();
            }
            
        }
    }

    public void ShowDialog(string text)
    {
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

            //tell game manager to continue game
        }
    }
}
