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
    }

    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
        dash.Disable();
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


    }

    public void Move()
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

    }

    public void Dash()
    {
        if (dashTimerCurCooldown > 0)
            return;

        dashTimerCurCooldown = dashTimerMaxCooldown;
        dashCurrentDuration = dashMaxDuration;
        isDashing = true;
    }

}
