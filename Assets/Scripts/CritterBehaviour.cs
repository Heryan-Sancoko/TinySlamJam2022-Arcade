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

    private Coroutine penningRoutine;
    private bool isAroundPen = false;
    [SerializeField]
    private float patienceTimer;
    private PenBehaviour creaturePen;

    [SerializeField]
    private SkinnedMeshRenderer mSkin;
    public SkinnedMeshRenderer critterSkin => mSkin;
    


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
                if (mHero == null)
                {
                    if (HeroManager.Instance != null)
                    {
                        if (HeroManager.Instance.heroInstance == null)
                        {
                            Debug.Log("WHY");
                        }
                        else
                        mHero = HeroManager.Instance.heroInstance;
                    }
                    else
                        return;
                }

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
                if (penningRoutine == null)
                {
                    WanderRandomly();
                    if (wanderIntent !=1)
                    {
                        mAnim.SetBool("isMoving", false);
                           currentState = critterState.idle;
                    }
                }
                break;

            case critterState.idle:
                if (penningRoutine == null)
                {
                    WanderRandomly();
                    if (wanderIntent == 1)
                    {
                        mAnim.SetBool("isMoving", true);
                        currentState = critterState.wander;
                    }
                }
                break;

            default:
                break;
        }

        if (isPenned)
        {
            patienceTimer -= Time.deltaTime;
            if (patienceTimer <= 0)
            {
                if (penningRoutine == null)
                    penningRoutine = StartCoroutine(UnpenSelf());

            }
        }

            //mAnim.SetBool("isMoving", (wanderDirection!=Vector3.zero && currentState!= critterState.held));


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
        if (penningRoutine != null)
        {
            return;
            //StopCoroutine(penningRoutine);
            //penningRoutine = null;
        }


        mAnim.SetBool("isMoving", false);
        mAnim.SetBool("isGrounded", true);

        currentState = critterState.held;
        launchCurDuration = 0;
        mAnim.SetBool("isLaunched", false);
        isPenned = false;

    }

    public void LaunchCritter()
    {
        if (GameScoreManager.Instance != null)
            OnTriggerExit(GameScoreManager.Instance.creaturePen.penTrigger);

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

        if (collision.gameObject.layer == 12)
        {
            if (!isPenned)
            {
                if (currentState == critterState.launched)
                {
                    if (penningRoutine == null)
                        penningRoutine = StartCoroutine(GetPenned());
                }
            }
        }

        if (collision.gameObject.layer == 3)
        {
            mAnim.SetBool("isGrounded", true);
            if (isPenned)
            {
                penningRoutine = null;
                mSkin.enabled = false;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 12)
        {
            if (!isPenned)
            {
                if (currentState == critterState.launched)
                {
                    if (penningRoutine == null)
                        penningRoutine = StartCoroutine(GetPenned());
                }
            }
        }

        if (collision.gameObject.layer == 3)
        {
            mAnim.SetBool("isGrounded", true);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            isAroundPen = false;
            isPenned = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.layer == 12)
        {
            isAroundPen = true;
            if (currentState == critterState.launched)
            {
                if (penningRoutine==null)
                penningRoutine = StartCoroutine(GetPenned());
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 12)
        {
            isAroundPen = true;
            if (currentState == critterState.launched)
            {
                if (penningRoutine == null)
                    penningRoutine = StartCoroutine(GetPenned());
            }
        }
    }

    private IEnumerator GetPenned()
    {
        isPenned = true;
        gameObject.layer = 14;
        patienceTimer = Random.Range(mCritterObject.patience - mCritterObject.patienceRange, mCritterObject.patience + mCritterObject.patienceRange);
        if (creaturePen==null)
        creaturePen = GameScoreManager.Instance.creaturePen;

        creaturePen.ScorePoint();
        transform.LookAt(creaturePen.transform.position);
        float jumpHeight = transform.position.y + 5;
        float originalHeight = transform.position.y;
        mAnim.SetBool("isPenned", true);
        mAnim.SetBool("isGrounded", false);
        mAnim.SetTrigger("Jump");

        while (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(creaturePen.transform.position.x, creaturePen.transform.position.z)) > 1f)
        {
            wanderDirection = Vector3.zero;
            rbody.velocity = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, jumpHeight, transform.position.z), 0.1f);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(creaturePen.transform.position.x, transform.position.y, creaturePen.transform.position.z), 0.1f);
            yield return null;
        }

        //penningRoutine = null;
        //mAnim.SetBool("isGrounded", true);
        yield return null;
    }

    private IEnumerator UnpenSelf()
    {
        if (creaturePen == null)
            creaturePen = GameScoreManager.Instance.creaturePen;

        creaturePen.SubtractPoints();
        mSkin.enabled = true;
        float jumpHeight = transform.position.y + 10;

        mAnim.SetBool("isGrounded", false);
        mAnim.SetTrigger("Jump");

        RandomizeWanderDirection(Vector3.zero);
        Vector3 jumpDestination = creaturePen.transform.position + (wanderDirection * 14);
        transform.LookAt(jumpDestination);
        while (isPenned)
        {
            rbody.velocity = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, jumpHeight, transform.position.z), 0.1f);
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(jumpDestination.x, transform.position.y, jumpDestination.z), 0.1f);
            if (mAnim.GetBool("isGrounded") == true)
            {
                isPenned = false;
            }
            yield return null;
        }

        gameObject.layer = 6;
        mAnim.SetBool("isPenned", false);
        penningRoutine = null;
        RandomizeWanderDirection(Vector3.zero);
        yield return null;
    }
}
