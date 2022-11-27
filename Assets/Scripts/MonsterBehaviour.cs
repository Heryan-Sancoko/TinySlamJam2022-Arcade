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
    [SerializeField]
    private CritterBehaviour targetedCritter;
    [SerializeField]
    private bool isHoldingTargetedCritter = false;
    [SerializeField]
    private Transform monsterHands;
    public Collider monsterCollider;

    private HeroBehaviour mhero;

    [SerializeField]
    private float maxLaunchTimer = 0.5f;
    [SerializeField]
    private float curLaunchTimer;
    [SerializeField]
    private float launchspeed;

    [SerializeField]
    private float maxStunnedTime = 1;
    [SerializeField]
    private float curStunnedTime = 0;

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

        if (nearbyCritters != null)
            nearbyCritters.Clear();
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
                if (!isHoldingTargetedCritter)
                {
                    if (nearbyCritters.Count > 1)
                    {
                        SearchForNearbyCritters();
                    }
                    else if (nearbyCritters.Count == 1)
                    {
                        targetedCritter = nearbyCritters[0];
                        if (targetedCritter.isPenned || targetedCritter.gameObject == null || targetedCritter.heldByMonster != null && targetedCritter.heldByMonster != this)
                        {
                            targetedCritter = null;
                            nearbyCritters.Clear();
                            currentState = monsterState.wandering;
                        }
                    }
                    else if (nearbyCritters.Count == 0)
                    {
                        currentState = monsterState.wandering;
                    }

                    if (targetedCritter != null)
                    {
                        Vector3 newDirection = Vector3.RotateTowards(transform.forward, (targetedCritter.transform.position - transform.position).normalized, (currentMoveSpeed * 3) * Time.deltaTime, 0.0f);
                        transform.rotation = Quaternion.LookRotation(newDirection);
                    }
                    else
                    {
                        currentState = monsterState.wandering;
                        return;
                    }

                    rbody.velocity = transform.forward * currentMoveSpeed;

                    if (Vector3.Distance(targetedCritter.transform.position, transform.position) < 3)
                    {
                        isHoldingTargetedCritter = true;
                    }

                }
                
                if (targetedCritter != null)
                {
                    if (isHoldingTargetedCritter)
                    {
                        if (targetedCritter.heldByMonster != this)
                        {
                            DropHeldCritter();
                            return;
                        }    

                        //if (!targetedCritter.isPenned && targetedCritter.heldByMonster == this)
                        //{
                            targetedCritter.PickupCritter(this);
                            targetedCritter.transform.position = monsterHands.transform.position;
                            targetedCritter.transform.rotation = monsterHands.rotation;
                            Vector3 middle = Vector3.zero;
                            middle.y = transform.position.y;
                            rbody.velocity = (transform.position - middle).normalized * currentMoveSpeed;
                        //}
                        //else
                        //{
                        //    Debug.LogError("DROPPING CRITTER");
                        //    isHoldingTargetedCritter = false;
                        //    DropHeldCritter();
                        //}
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
                if (mhero.heldMonster == this)
                {
                    DropHeldCritter();
                    transform.position = mhero.heldTransform.position;
                    transform.rotation = mhero.heldTransform.rotation;
                }
                else
                {
                    currentState = monsterState.wandering;
                }
                break;
            case monsterState.launched:

                DropHeldCritter();

                gameObject.layer = 10;
                mhero.heldMonster = null;
                rbody.velocity = transform.forward * launchspeed;
                curLaunchTimer -= Time.deltaTime;

                if (curLaunchTimer <= 0)
                {
                    gameObject.layer = 8;
                    if (IsOutsideScreen())
                    {
                        SetUpMonster();
                    }
                    else
                    {
                        if (curStunnedTime > 0)
                        {
                            currentState = monsterState.stunned;
                        }
                        else
                        {
                            currentState = monsterState.wandering;
                        }
                    }
                }
                break;
            case monsterState.stunned:
                curStunnedTime -= Time.deltaTime;
                if (curStunnedTime <= 0)
                    currentState = monsterState.wandering;
                break;
            default:
                break;
        }

    }


    private void SearchForNearbyCritters()
    {
        if (nearbyCritters.Count > 1)
        {
            List<CritterBehaviour> critterlist = nearbyCritters.ToList();
            foreach (CritterBehaviour critter in critterlist)
            {
                if (critter == null)
                {
                    nearbyCritters.Remove(critter);
                    continue;
                }
            
                if (critter.isPenned || critter.gameObject == null || critter.heldByMonster != null && critter.heldByMonster != this)
                {
                    if (targetedCritter == critter)
                    {
                        targetedCritter = null;
                    }
            
                    nearbyCritters.Remove(critter);
                }
            }
        }
        else
        {
            if (nearbyCritters[0] == null)
            {
                nearbyCritters.Clear();
            }
            else if (nearbyCritters[0].isPenned || nearbyCritters[0].gameObject == null || nearbyCritters[0].heldByMonster != null && nearbyCritters[0].heldByMonster != this)
            {
                if (targetedCritter == nearbyCritters[0])
                {
                    targetedCritter = null;
                }

                nearbyCritters.Clear();
            }
        }

        if (nearbyCritters.Count > 1)
            targetedCritter = nearbyCritters.OrderBy(x => Vector2.Distance(transform.position, x.transform.position)).ToList()[0];

        if (nearbyCritters.Count == 0)
        {
            currentState = monsterState.wandering;
            return;
        }

    }

    public void DropHeldCritter()
    {
        Debug.LogError(gameObject.name + "Dropping critter!");
        isHoldingTargetedCritter = false;
        if (targetedCritter != null)
        {
            //targetedCritter.DropCritter();
            targetedCritter = null;
        }

        if (currentState != monsterState.grabbed && currentState != monsterState.launched)
        {
            StunMe(0.5f);
            currentState = monsterState.stunned;
        }
    }

    public bool IsOutsideScreen()
    {
        return (transform.position.x > 30 || transform.position.x < -30 || transform.position.z > 30 || transform.position.z < -30);
    }

    public void CatchTargetedCritter(CritterBehaviour critter)
    {
        if (currentState == monsterState.stunned)
            return;

        if (mhero.heldCritter == critter)
            mhero.DropCritter();

        targetedCritter = critter;
        critter.PickupCritter(this);
        isHoldingTargetedCritter = true;
    }

    public void GrabMonster()
    {
        if (currentState == monsterState.launched)
            return;


        if (mhero.heldMonster != null || mhero.heldCritter != null)
            return;

        mhero.heldMonster = this;
        gameObject.layer = 15;
        currentState = monsterState.grabbed;
    }

    public void LaunchEnemy()
    {
        currentState = monsterState.launched;
        gameObject.layer = 10;
        curLaunchTimer = maxLaunchTimer;
        mhero.heldMonster = null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8 && currentState == monsterState.launched)
        {
            MonsterBehaviour hitMonster = collision.gameObject.GetComponent<MonsterBehaviour>();
            LaunchEnemy();
            Vector3 launchDir = transform.forward + ((transform.position - collision.transform.position).normalized*0.5f);
            launchDir.y = 0;
            launchDir.Normalize();

            hitMonster.LaunchEnemy();
            hitMonster.transform.LookAt(hitMonster.transform.position + (transform.forward + (hitMonster.transform.position - transform.position).normalized).normalized * launchspeed);

            StunMe(maxStunnedTime);
            hitMonster.StunMe(maxStunnedTime);

            transform.LookAt(transform.position + (launchDir * launchspeed));
        }
    }

    public void StunMe(float newStunTime)
    {
        curStunnedTime = newStunTime; ;
    }

    public void AddToCritterList(CritterBehaviour newCritter)
    {
        gameObject.layer = 8;

        if (!nearbyCritters.Contains(newCritter))
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
