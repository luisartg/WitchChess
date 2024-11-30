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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseOver()
    {
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

        sectionVisual.transform.position = clampWP;
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
}
