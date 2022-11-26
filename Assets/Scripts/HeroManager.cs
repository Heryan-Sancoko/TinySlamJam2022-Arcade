using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    private static HeroManager _instance;

    public HeroBehaviour heroInstance;

    public static HeroManager Instance
    {
        get 
        {
            if (_instance == null)
            {
                Debug.LogError("HeroManager is null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
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
