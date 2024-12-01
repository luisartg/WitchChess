using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapSpecialEffect : MonoBehaviour, ISpecialEffect
{
    public void StartSpecialEffect()
    {
        FindObjectOfType<GameManager>().AddPlayerMoveCounter();
        FindObjectOfType<GameManager>().PrepareForPlayerSwap();
        Debug.Log("Player awarded one more move and can swap");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
