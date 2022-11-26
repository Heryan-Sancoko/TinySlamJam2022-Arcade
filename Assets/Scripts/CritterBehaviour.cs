using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum critterState
{
    held,
    idle,
    wander,
    roosting,
    launched
}

public class CritterBehaviour : EntityBehaviour
{

    public CritterObject mCritterObject;
    public Animator mAnim;
    public critterState currentState;
    public float launchVelocity = 20;
    [SerializeField]
    private float launchMaxDuration = 0.5f;
    private float launchCurDuration = 0;
    private HeroBehaviour mHero;
    private Vector3 wanderDestination;
    private List<ProjectileBehaviour> listOfBullets = new List<ProjectileBehaviour>();
    [SerializeField]
    private ProjectileBehaviour bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        mHero = HeroManager.Instance.heroInstance;
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case critterState.held:
                transform.position = mHero.heldTransform.position;
                transform.rotation = mHero.heldTransform.rotation;
                break;
            case critterState.launched:
                launchCurDuration -= Time.deltaTime;
                if (launchCurDuration<=0)
                {
                    launchCurDuration = 0;
                    rbody.velocity = Vector3.zero;
                    currentState = critterState.idle;
                    mAnim.SetBool("isLaunched", false);
                }
                break;
            case critterState.wander:
                break;
            default:
                break;
        }

    }

    public void Shoot()
    {
        if (listOfBullets == null)
        {
            listOfBullets = new List<ProjectileBehaviour>();
        }

        ProjectileBehaviour newBullet = listOfBullets.Find(x => x.gameObject.activeSelf == false);

        if (newBullet == null)
        {
            newBullet = Instantiate(bulletPrefab, transform.position + transform.forward, transform.rotation);
            listOfBullets.Add(newBullet);
        }
        else
        {
            newBullet.transform.position = transform.position + transform.forward;
            newBullet.transform.rotation = transform.rotation;
            newBullet.gameObject.SetActive(true);
        }



    }


    public void LaunchCritter()
    {
        currentState = critterState.launched;
        launchCurDuration = launchMaxDuration;
        rbody.velocity = transform.forward * launchVelocity;
        mAnim.SetBool("isLaunched", true);
    }
}
