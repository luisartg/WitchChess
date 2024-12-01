using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEffects : MonoBehaviour
{
    public GameObject sectionVisual;

    [SerializeField]
    private Vector3 mousePosition;

    private Plane boardPlane;
    private bool planeInitialized = false;

    private GameManager gameManager;

    private bool playerPositionNeedsUpdate = true;
    private Vector2 currentPlayerPos;
    private BoardManager boardManager;

    private bool mouseEffectActive = true;

    // Start is called before the first frame update
    void Start()
    {
        sectionVisual.SetActive(false);
        gameManager = FindFirstObjectByType<GameManager>();
        boardManager = FindFirstObjectByType<BoardManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActiveMouseEffect(bool isActive)
    {
        mouseEffectActive = isActive;
        if (!isActive)
        {
            sectionVisual.SetActive(false);
        }
    }

    void OnMouseOver()
    {
        if (!mouseEffectActive)
        {
            return;
        }

        UpdatePlayerPos();
        /*
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.y);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        Debug.Log($"MousePos: {mousePos}");
        Debug.Log($"WorldMousePos: {worldPosition}");
        */

        Vector3 prevWP = GetWorldPositionOnLocalPlane();
        Vector3 clampWP = new(Mathf.RoundToInt(prevWP.x),
                              Mathf.RoundToInt(prevWP.y),
                              Mathf.RoundToInt(prevWP.z));

        if (MouseIsInsideConstraints(currentPlayerPos, clampWP))
        {
            sectionVisual.SetActive(true);
            sectionVisual.transform.position = clampWP;
        }
        else
        {
            MakeSelectionDissappear();
        }
    }

    private void UpdatePlayerPos()
    {
        if (playerPositionNeedsUpdate)
        {
            currentPlayerPos = boardManager.GetCurrentPlayerPosition();
            playerPositionNeedsUpdate = false;
        }
    }

    public void ForcedUpdatePlayerPos()
    {
        currentPlayerPos = boardManager.GetCurrentPlayerPosition();
        playerPositionNeedsUpdate = false;
    }

    private void MakeSelectionDissappear()
    {
        sectionVisual.SetActive(false);
    }

    private bool MouseIsInsideConstraints(Vector2 currentPlayerPosition, Vector3 clampWP)
    {
        if (currentPlayerPosition.x == clampWP.x && currentPlayerPosition.y == clampWP.z)
        {
            //Do not allow if mouse is in same place as the player piece
            return false;
        }

        int xi, xf, yi, yf;
        xi = (int)currentPlayerPosition.x - 1;
        xf = (int)currentPlayerPosition.x + 1;
        yi = (int)currentPlayerPosition.y - 1;
        yf = (int)currentPlayerPosition.y + 1;

        xi = Mathf.Clamp(xi, 0, 5);
        xf = Mathf.Clamp(xf, 0, 5);
        yi = Mathf.Clamp(yi, 0, 5);
        yf = Mathf.Clamp(yf, 0, 5);

        if (clampWP.x >= xi && clampWP.x <= xf
            && clampWP.z >= yi && clampWP.z <= yf)
        {
            return true;
        }
        return false;
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        Debug.Log("Mouse is no longer on GameObject.");
    }

    private Vector3 GetWorldPositionOnPlane()
    {
        Vector3 screenPosition = Input.mousePosition;
        float height = 0;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(Vector3.up, new Vector3(ray.origin.x, height, ray.origin.z));

        float distance;
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    private Vector3 GetWorldPositionOnLocalPlane()
    {
        Vector3 screenPosition = Input.mousePosition;
        float height = 0;
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (!planeInitialized)
        {
            boardPlane = new Plane(Vector3.up, new Vector3(ray.origin.x, height, ray.origin.z));
        }

        float distance;
        boardPlane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    private void OnMouseDown()
    {
        if (!mouseEffectActive)
        {
            return;
        }

        Debug.Log("Click made!");
        if (sectionVisual.activeSelf == true)
        {
            // position is valid
            Vector2 newPos = new Vector2(sectionVisual.transform.position.x, sectionVisual.transform.position.z);
            gameManager.MovePlayerPieceTo(newPos);
            currentPlayerPos = newPos;
            Debug.Log($"New player pos is {newPos}");
        }
    }
}
