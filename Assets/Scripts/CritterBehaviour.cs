using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum critterState
{
    held,
    idle,
    wander,
    launched
}

public class CritterBehaviour : WanderingBehaviour
{

    public CritterObject mCritterObject;
    public Animator mAnim;
    public critterState currentState;
    public bool isPenned = false;
    public float launchVelocity = 20;
    [SerializeField]
    private float launchMaxDuration = 0.5f;
    private float launchCurDuration = 0;
    private HeroBehaviour mHero;
    private List<ProjectileBehaviour> listOfBullets = new List<ProjectileBehaviour>();
    [SerializeField]
    private ProjectileBehaviour bulletPrefab;
    [SerializeField]
    private float shootMaxCooldown = 0.2f;
    private float shootCurCooldown = 0;
    
    


    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        mHero = HeroManager.Instance.heroInstance;
    }

    // Update is called once per frame
    void Update()
    {
        shootCurCooldown -= Time.deltaTime;

        switch (currentState)
        {
            case critterState.held:
                transform.position = mHero.heldTransform.position;
                transform.rotation = mHero.heldTransform.rotation;

                if (isPenned)
                    currentState = critterState.idle;
                break;

            case critterState.launched:
                launchCurDuration -= Time.deltaTime;
                if (launchCurDuration <= 0 || isPenned)
                {
                    launchCurDuration = 0;
                    rbody.velocity = Vector3.zero;
                    currentState = critterState.idle;
                    mAnim.SetBool("isLaunched", false);

                    if (isPenned)
                        currentState = critterState.idle;
                }
                break;

            case critterState.wander:
                WanderRandomly();
                if (rbody.velocity.magnitude == 0)
                    currentState = critterState.idle;
                break;

            case critterState.idle:
                WanderRandomly();
                if (rbody.velocity.magnitude > 0)
                    currentState = critterState.wander;
                break;

            default:
                break;
        }

        mAnim.SetBool("isMoving", (rbody.velocity.magnitude > 0 && currentState!= critterState.held));

    }

    public void Shoot()
    {
        if (shootCurCooldown <= 0)
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
            shootCurCooldown = shootMaxCooldown;
        }
    }

    public void PickupCritter()
    {
        currentState = critterState.held;
        launchCurDuration = 0;
        mAnim.SetBool("isLaunched", false);

    }

    public void LaunchCritter()
    {
        currentState = critterState.launched;
        launchCurDuration = launchMaxDuration;
        rbody.velocity = transform.forward * launchVelocity;
        mAnim.SetBool("isLaunched", true);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            if (currentState == critterState.wander)
            {
                wanderdirConfigureCooldown = 0;
                RandomizeWanderDirection(collision.contacts[0].normal);
            }
        }
    }
}
