using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScoreManager : MonoBehaviour
{

    private static GameScoreManager _instance;

    public HeroBehaviour heroInstance;
    public PenBehaviour creaturePen;
    private int pointsScored = 0;

    [SerializeField]
    private Text pointsText;

    public static GameScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameScoreManager is null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }


    public void ScorePoints(int additionalPoints)
    {
        pointsScored += additionalPoints;
        pointsText.text = pointsScored.ToString();
    }
    public void SubtractPoints(int pointsLost)
    {
        pointsScored -= pointsLost;
        pointsText.text = pointsScored.ToString();
    }
}
