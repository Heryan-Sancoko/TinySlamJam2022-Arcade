using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameScoreManager : MonoBehaviour
{

    private static GameScoreManager _instance;

    public HeroBehaviour heroInstance;
    public PenBehaviour creaturePen;
    private int pointsScored = 0;
    [SerializeField]
    private int pointsToWin;
    public int PintsToWin => pointsToWin;

    [SerializeField]
    private Text pointsText;

    [SerializeField]
    private List<CritterBehaviour> critterList = new List<CritterBehaviour>();
    private int spawnedCritterAmount = 0;

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
        SpawnCritter();
        if (pointsToWin <= pointsScored)
        {
            if (SceneManager.sceneCount != SceneManager.GetActiveScene().buildIndex)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            else
                SceneManager.LoadScene(0);
            //you win!
        }
    }
    public void SubtractPoints(int pointsLost)
    {
        pointsScored -= pointsLost;
        pointsText.text = pointsScored.ToString();
    }

    public void SpawnCritter()
    {
        if (spawnedCritterAmount == pointsToWin)
            return;

        CritterBehaviour newCritter = Instantiate(critterList[Random.Range(0, critterList.Count)], null);

        float spawnZ = Random.Range
            (-20, 20);
        float spawnX = Random.Range
            (-17, 17);

        Vector3 spawnPosition = new Vector3(spawnX, 0, spawnZ);
        newCritter.transform.position = spawnPosition;
        spawnedCritterAmount++;

    }
}
