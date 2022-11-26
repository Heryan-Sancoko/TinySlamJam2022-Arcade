using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PenBehaviour : MonoBehaviour
{
    private GameScoreManager gsm;
    public Collider penTrigger;
    public LayerMask SeeTheseLayers;

    public List<Collider> penWalls = new List<Collider>();

    private void Start()
    {
        ConfigurePenPos();

        penTrigger = GetComponent<BoxCollider>();
        if (GameScoreManager.Instance !=null)
        gsm = GameScoreManager.Instance;
    }

    private void ConfigurePenPos()
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

        RaycastHit[] inMyZone = Physics.BoxCastAll(transform.position + (Vector3.up * 2), penTrigger.bounds.extents * 0.6f, Vector3.down, transform.rotation, 999, SeeTheseLayers);

        List<RaycastHit> hitThings = inMyZone.ToList();

        if (hitThings.Exists(x => x.transform.gameObject.layer == 6) || hitThings.Exists(x => x.transform.gameObject.layer == 13))
        {
            ConfigurePenPos();
        }
        else
        {
            foreach (RaycastHit thing in hitThings)
            {
                Debug.LogError(thing.transform.name);
                if (thing.transform.gameObject.layer == 6 || thing.transform.gameObject.layer == 13)
                {
                    Debug.LogError("DO NOT DELETE THIS THNG: " + thing.transform.name);
                }
                else
                    thing.transform.gameObject.SetActive(false);
            }
        }
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
