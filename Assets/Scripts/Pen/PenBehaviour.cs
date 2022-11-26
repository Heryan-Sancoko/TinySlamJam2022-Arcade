using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenBehaviour : MonoBehaviour
{
    private GameScoreManager gsm;
    public Collider penTrigger;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            float spawnZ = Random.Range
                (-20, 20);
            float spawnX = Random.Range
                (-17, 17);

            Vector3 spawnPosition = new Vector3(spawnX, transform.position.y, spawnZ);
            transform.position = spawnPosition;
        }

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
