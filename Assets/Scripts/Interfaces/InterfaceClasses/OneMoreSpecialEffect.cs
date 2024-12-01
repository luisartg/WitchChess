using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneMoreSpecialEffect : MonoBehaviour, ISpecialEffect
{
    public void StartSpecialEffect()
    {
        FindObjectOfType<GameManager>().AddPlayerMoveCounter();
        Debug.Log("Player awarded one more move");
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
