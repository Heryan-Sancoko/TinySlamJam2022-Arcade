using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenBehaviour : MonoBehaviour
{
    private GameScoreManager gsm;
    public Collider penTrigger;

    private void Start()
    {
        penTrigger = GetComponent<BoxCollider>();
        if (GameScoreManager.Instance !=null)
        gsm = GameScoreManager.Instance;
    }

    public void ScorePoint(int pointsScored = 1)
    {
        gsm.ScorePoints(pointsScored);
    }

    public void SubtractPoints(int pointsLost = 1)
    {
        gsm.SubtractPoints(pointsLost);
    }

}
