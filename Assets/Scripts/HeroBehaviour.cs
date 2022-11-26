using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeroBehaviour : EntityBehaviour
{

    private PlayerControls mInput;
    [SerializeField]
    private Vector2 moveDir = Vector2.zero;

    private InputAction move;
    private InputAction fire;
    private InputAction dash;
    private InputAction pickUpThrow;

    //--------------------for movement
    [SerializeField]
    private float maxMoveSpeed;
    [SerializeField]
    private float currentMoveSpeed;

    [SerializeField]
    private float dashTimerMaxCooldown;
    [SerializeField]
    private float dashTimerCurCooldown;
    [SerializeField]
    private float dashMaxDuration;
    [SerializeField]
    private float dashCurrentDuration;
    [SerializeField]
    private float dashSpeed;

    private bool isDashing = false;

    //-------------------for critters
    public Transform heldTransform;
    [SerializeField]
    private CritterBehaviour heldCritter;
    public MonsterBehaviour heldMonster;
    [SerializeField]
    private float pickupRange;
    private bool successfulClickThisFrame = false;


    //--------------------for mousePos
    public Vector3 worldMousePos;
    public LayerMask lookLayer;


    private void Awake()
    {
        mInput = new PlayerControls();
    }

    private void OnEnable()
    {
        move = mInput.Player.Move;
        move.Enable();
        fire = mInput.Player.Fire;
        fire.Enable();
        dash = mInput.Player.Dash;
        dash.Enable();
        pickUpThrow = mInput.Player.PickupAndThrow;
        pickUpThrow.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
        dash.Disable();
        pickUpThrow.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Look();
        Shoot();
    }
    private void LateUpdate()
    {
        successfulClickThisFrame = false;
    }

    private void Shoot()
    {
        if (fire.WasReleasedThisFrame() && heldCritter != null)
        {
            heldCritter.Shoot();
        }
    }

    private void ThrowHeld()
    {
        if (pickUpThrow.WasReleasedThisFrame() && !successfulClickThisFrame)
        {
            if (heldCritter != null)
            {
                heldCritter.LaunchCritter();
                heldCritter = null;
                successfulClickThisFrame = true;
            }

            if (heldMonster != null)
            {
                heldMonster.LaunchEnemy();
                heldMonster = null;
                successfulClickThisFrame = true;
            }
        }
    }

    private void Look()
    {
        ThrowHeld();

        heldTransform.gameObject.SetActive(heldCritter != null && heldMonster != null);


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, 999999, lookLayer))
        {
            worldMousePos = hitData.point;
            worldMousePos.y = transform.position.y;

            if (pickUpThrow.WasReleasedThisFrame() && heldCritter == null && heldMonster == null)
            {
                switch (hitData.collider.gameObject.layer)
                {
                    case 6:
                        if (hitData.collider.gameObject.TryGetComponent(out CritterBehaviour newCritter))
                        {
                            Debug.LogError("CRITTER!");
                            //looking at a critter
                            if (Vector3.Distance(transform.position, newCritter.transform.position) < pickupRange)
                            {

                                if (!successfulClickThisFrame)
                                {
                                    PickupCritter(newCritter);
                                    successfulClickThisFrame = true;
                                }
                            }
                        }
                        break;
                    case 8:
                        if (hitData.collider.gameObject.TryGetComponent(out MonsterBehaviour newMonster))
                        {
                            Debug.LogError("MONSTER!");
                            //looking at a monster
                            if (Vector3.Distance(transform.position, newMonster.transform.position) < pickupRange)
                            {
                                if (!successfulClickThisFrame)
                                {
                                    PickupMonster(newMonster);
                                    successfulClickThisFrame = true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            RotateToMousePosition();
        }
    }

    private void PickupCritter(CritterBehaviour newCritter)
    {
        if (newCritter.isPenned || newCritter.critterSkin.enabled == false || heldMonster!=null)
            return;

        heldCritter = newCritter;
        newCritter.PickupCritter(true);
    }

    private void PickupMonster(MonsterBehaviour newMonster)
    {
        if (heldCritter != null)
            return;

        heldMonster = newMonster;
        newMonster.GrabMonster();
    }

    private void Move()
    {
        if (!isDashing)
        {
            moveDir = move.ReadValue<Vector2>();

            if (moveDir.magnitude != 0)
            {
                if (moveDir.y > 0.25f)
                    moveDir.y = 1;
                else if (moveDir.y < -0.25f)
                    moveDir.y = -1;
                else
                    moveDir.y = 0;

                if (moveDir.x > 0.25f)
                    moveDir.x = 1;
                else if (moveDir.x < -0.25f)
                    moveDir.x = -1;
                else
                    moveDir.x = 0;

                moveDir = moveDir.normalized;
            }

            rbody.velocity = new Vector3(moveDir.x, 0, moveDir.y) * currentMoveSpeed;


            if (dashTimerCurCooldown > 0)
            {
                dashTimerCurCooldown -= Time.deltaTime;
            }
        }
        else
        {
            if (moveDir.magnitude != 0)
                rbody.velocity = new Vector3(moveDir.x, 0, moveDir.y) * (dashSpeed);
            else
                rbody.velocity = Vector3.zero;

            dashCurrentDuration -= Time.deltaTime;
            if (dashCurrentDuration <= 0)
                isDashing = false;
        }


        if (dash.WasPressedThisFrame())
        {
            Dash();
        }

        //RotateToMoveDir();
    }

    private void RotateToMoveDir()
    {
        if (rbody.velocity.magnitude!=0)
        transform.LookAt(transform.position + ((rbody.velocity).normalized*10));

    }

    private void RotateToMousePosition()
    {
        transform.LookAt(worldMousePos);
    }

    private void Dash()
    {
        if (dashTimerCurCooldown > 0)
            return;

        dashTimerCurCooldown = dashTimerMaxCooldown;
        dashCurrentDuration = dashMaxDuration;
        isDashing = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6 && heldCritter==null)
        {
            if (collision.gameObject.TryGetComponent(out CritterBehaviour newCritter))
            {
                PickupCritter(newCritter);
            }
        }
    }
}
