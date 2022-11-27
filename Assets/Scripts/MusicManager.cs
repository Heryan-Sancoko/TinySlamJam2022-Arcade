using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager _instance;

    public List<AudioSource> MusicList;
    public float fadeincriment;
    public Coroutine fadeRoutine;

    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("MusicManager is null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        //fadeRoutine = StartCoroutine(FadeInFadeOut(music01));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeInFadeOutMusic(int musicIn, int musicOut = 0)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
            fadeRoutine = StartCoroutine(FadeInFadeOut(MusicList[musicIn-1], (musicOut == 0) ? null : MusicList[musicOut-1]));
        }
        else
        {
            fadeRoutine = StartCoroutine(FadeInFadeOut(MusicList[musicIn-1], (musicOut == 0) ? null : MusicList[musicOut-1]));
        }
    }

    public IEnumerator FadeInFadeOut(AudioSource musicIn, AudioSource musicOut = null)
    {
        while (musicIn.volume < 0.99f)
        {
            yield return new WaitForEndOfFrame();
            musicIn.volume = Mathf.MoveTowards(musicIn.volume, 1, fadeincriment * Time.deltaTime);
            if (musicOut != null)
            {
                musicOut.volume = Mathf.MoveTowards(musicOut.volume, 0, fadeincriment * Time.deltaTime);
            }
            yield return null;
        }

        if (fadeRoutine != null)
        {
            fadeRoutine = null;
        }
    }
}
