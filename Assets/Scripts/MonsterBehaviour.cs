using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum monsterState
{
    entering,
    wandering,
    chasing,
    grabbed,
    launched,
    stunned
}

public class MonsterBehaviour : WanderingBehaviour
{
    [SerializeField]
    private monsterState currentState;
    [SerializeField]
    private List<CritterBehaviour> nearbyCritters = new List<CritterBehaviour>();
    private CritterBehaviour targetedCritter;
    private bool isHoldingTargetedCritter = false;
    [SerializeField]
    private Transform monsterHands;
    public Collider monsterCollider;

    private HeroBehaviour mhero;

    [SerializeField]
    private float maxLaunchTimer = 0.5f;
    private float curLaunchTimer;
    [SerializeField]
    private float launchspeed;


    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        gameObject.layer = 2;

        SetUpMonster();

        dontIdleWhileWandering = true;

        if (HeroManager.Instance != null)
        {
            mhero = HeroManager.Instance.heroInstance;
        }
    }

    public void SetUpMonster()
    {
        float spawnZ = Random.Range
            (-30, 30);
        float spawnX = Random.Range
            (-25, 25);

        Vector3 spawnPosition = new Vector3(spawnX, 0, spawnZ);
        spawnPosition = spawnPosition.normalized * 30;
        spawnPosition.y = transform.position.y;
        transform.position = spawnPosition;

        Vector3 middle = Vector3.zero;
        middle.y = transform.position.y;

        transform.LookAt(middle);

        currentState = monsterState.entering;
    }

    // Update is called once per frame
    void Update()
    {

        switch (currentState)
        {
            case monsterState.entering:
                rbody.velocity = transform.forward * currentMoveSpeed;

                if (Vector3.Distance(Vector3.zero, transform.position) < 10 || targetedCritter != null)
                {
                    if (Vector3.Distance(Vector3.zero, transform.position) < 10)
                    {
                        gameObject.layer = 8;
                        currentState = monsterState.wandering;
                    }

                    if (targetedCritter != null)
                    {
                        gameObject.layer = 8;
                        currentState = monsterState.chasing;
                    }
                }
                break;
            case monsterState.wandering:
                WanderRandomly();
                if (nearbyCritters.Count > 0)
                {
                    if (nearbyCritters.Count == 1)
                    {
                        targetedCritter = nearbyCritters[0];
                        currentState = monsterState.chasing;
                    }
                    else
                    {
                        //nearbyCritters.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position));
                        targetedCritter = nearbyCritters.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList()[0];
                        currentState = monsterState.chasing;
                    }
                }

                if (IsOutsideScreen())
                {
                    SetUpMonster();
                }

                break;
            case monsterState.chasing:
                if (targetedCritter != null)
                {
                    if (targetedCritter == null || !isHoldingTargetedCritter)
                    {
                        if (nearbyCritters.Count > 1)
                        {
                            List<CritterBehaviour> critterlist = nearbyCritters.ToList();
                            foreach (CritterBehaviour critter in critterlist)
                            {
                                if (critter.isPenned || critter.currentState == critterState.held || critter.gameObject==null)
                                {
                                    nearbyCritters.Remove(critter);
                                }
                                else
                                {
                                    if (targetedCritter == null)
                                    {
                                        targetedCritter = critter;
                                    }
                                    else
                                    {
                                        if (Vector3.Distance(critter.transform.position, transform.position) < Vector3.Distance(targetedCritter.transform.position, transform.position))
                                        {
                                            targetedCritter = critter;
                                        }
                                    }
                                }
                            }

                            //targetedCritter = nearbyCritters.OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).ToList()[0];
                        }
                        else if (nearbyCritters.Count == 1)
                        {
                            targetedCritter = nearbyCritters[0];
                        }
                        else if (nearbyCritters.Count == 0)
                        {
                            currentState = monsterState.wandering;
                        }

                        Vector3 newDirection = Vector3.RotateTowards(transform.forward, (targetedCritter.transform.position - transform.position).normalized, (currentMoveSpeed * 3) * Time.deltaTime, 0.0f);
                        transform.rotation = Quaternion.LookRotation(newDirection);

                        rbody.velocity = transform.forward * currentMoveSpeed;

                        if (Vector3.Distance(targetedCritter.transform.position, transform.position) < 3)
                        {
                            isHoldingTargetedCritter = true;
                        }

                    }

                    if (isHoldingTargetedCritter)
                    {
                        targetedCritter.PickupCritter(false);
                        targetedCritter.transform.position = monsterHands.transform.position;
                        targetedCritter.transform.rotation = monsterHands.rotation;
                        Vector3 middle = Vector3.zero;
                        middle.y = transform.position.y;
                        rbody.velocity = (transform.position - middle).normalized * currentMoveSpeed;
                    }

                    //TODO: what happens when the monster takes a critter outside the screen?

                    if (IsOutsideScreen())
                    {
                        GameScoreManager gsm = GameScoreManager.Instance;
                        gsm.LoseHeart();
                        gsm.spawnedCritterAmount--;
                        Destroy(targetedCritter.gameObject);
                        targetedCritter = null;
                        isHoldingTargetedCritter = false;
                        nearbyCritters.Clear();
                        SetUpMonster();
                    }
                }
                break;
            case monsterState.grabbed:
                if (targetedCritter != null)
                {
                    targetedCritter.DropCritter();
                    targetedCritter = null;
                    nearbyCritters.Clear();
                    isHoldingTargetedCritter = false;
                }
                transform.position = mhero.heldTransform.position;
                transform.rotation = mhero.heldTransform.rotation;
                break;
            case monsterState.launched:
                mhero.heldMonster = null;
                rbody.velocity = transform.forward * launchspeed;
                curLaunchTimer -= Time.deltaTime;

                if (curLaunchTimer <= 0)
                {
                    if (IsOutsideScreen())
                    {
                        SetUpMonster();
                    }
                    else
                    {
                        currentState = monsterState.wandering;
                    }
                }
                break;
            default:
                break;
        }

    }

    public bool IsOutsideScreen()
    {
        return (transform.position.x > 30 || transform.position.x < -30 || transform.position.z > 30 || transform.position.z < -30);
    }

    public void CatchTargetedCritter(CritterBehaviour critter)
    {
        targetedCritter = critter;
        isHoldingTargetedCritter = true;
    }

    public void GrabMonster()
    {
        if (currentState == monsterState.launched)
            return;

        gameObject.layer = 15;
        currentState = monsterState.grabbed;
    }

    public void LaunchEnemy()
    {
        currentState = monsterState.launched;
        gameObject.layer = 8;
        curLaunchTimer = maxLaunchTimer;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 && currentState == monsterState.launched)
        {
            MonsterBehaviour hitMonster = collision.gameObject.GetComponent<MonsterBehaviour>();
            LaunchEnemy();
            Vector3 launchDir = transform.forward + (transform.position - collision.transform.position).normalized;
            launchDir.y = 0;
            launchDir.Normalize();

            hitMonster.LaunchEnemy();
            hitMonster.transform.LookAt(hitMonster.transform.position + (transform.forward + (hitMonster.transform.position - transform.position).normalized).normalized * launchspeed);

            transform.LookAt(transform.position + (launchDir * launchspeed));


        }
    }

    public void AddToCritterList(CritterBehaviour newCritter)
    {
        gameObject.layer = 8;
        nearbyCritters.Add(newCritter);
    }

    public void RemoveFromCritterList(CritterBehaviour lostCritter)
    {
        if (nearbyCritters.Contains(lostCritter))
        {
            nearbyCritters.Remove(lostCritter);
        }
        
    }


}
