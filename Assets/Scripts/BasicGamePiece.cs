using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGamePiece : MonoBehaviour
{
    public float IntroductionHeight = 4;
    public float MoveSpeed = 0.5f;
    public float baseY = 0;

    public int PieceID;

    private ISpecialEffect specialEffect = null;

    private void Start()
    {
        specialEffect = GetComponent<ISpecialEffect>();

        SetStartPosition();
    }

    private void SetStartPosition()
    {
        Vector3 initialPos = transform.position;
        initialPos.y = IntroductionHeight;
        transform.position = initialPos;
    }

    public void RemoveThisPiece()
    {
        if (specialEffect != null)
        {
            Debug.Log($"{this.gameObject.name} special effect executed");
            specialEffect.StartSpecialEffect();
        }
        else
        {
            Debug.Log($"{this.gameObject.name} has no special effect");
        }

        DoLeave();
    }

    public void DoIntroduction()
    {
        //piece falls from sky
        SetStartPosition();
        StartCoroutine(FallDown());
    }

    private IEnumerator FallDown()
    {
        Vector3 currentPos = transform.position;
        while (transform.position.y > baseY)
        {
            currentPos.y -= MoveSpeed* Time.deltaTime;
            transform.position = currentPos; 
            yield return new WaitForEndOfFrame();
        }
        //TODO: PLAY SOUND OF PIECE
    }

    private void DoLeave()
    {
        StartCoroutine(GoUp());
    }

    private IEnumerator GoUp()
    {
        Vector3 currentPos = transform.position;
        while (transform.position.y < IntroductionHeight)
        {
            currentPos.y += MoveSpeed * Time.deltaTime;
            transform.position = currentPos;
            yield return new WaitForEndOfFrame();
        }

        //DESTROY GAME PIECE
        this.gameObject.transform.parent = null;
        Destroy(this.gameObject);
    }
}
