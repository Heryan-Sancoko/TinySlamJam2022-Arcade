using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderingBehaviour : EntityBehaviour
{
    [SerializeField]
    protected bool dontIdleWhileWandering = false;
    [SerializeField]
    protected Vector3 wanderDirection;
    [SerializeField]
    protected Vector3 mySpeed;
    [SerializeField]
    protected float maxWanderTime;
    [SerializeField]
    protected float minWanderTime;
    protected float wanderTimer;
    protected int wanderIntent;
    protected bool isMoving;
    [SerializeField]
    protected float maxMoveSpeed;
    [SerializeField]
    protected float currentMoveSpeed;
    [SerializeField]
    protected LayerMask lookForwardMask;
    protected float wanderdirConfigureCooldown = 0;

    protected void WanderRandomly()
    {
        if (wanderdirConfigureCooldown > 0)
            wanderdirConfigureCooldown -= Time.deltaTime;

        if (wanderTimer <= 0)
        {
            wanderIntent = Random.Range(1, 3);
            RandomizeWanderDirection(Vector3.zero);
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        }
        else
            wanderTimer -= Time.deltaTime;

        if (wanderIntent == 1 || dontIdleWhileWandering)
        {
            isMoving = true;
            if (Physics.Raycast(transform.position + ((-transform.forward) * 0.5f), transform.forward, out RaycastHit hitinfo, 3, lookForwardMask))
            {
                RandomizeWanderDirection(hitinfo.normal);
            }
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, wanderDirection, (currentMoveSpeed*3)*Time.deltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);
            Vector3 newSpeed = mySpeed = transform.forward * currentMoveSpeed;
            rbody.velocity = mySpeed = ChangeVelocityWithGravity(transform.forward * currentMoveSpeed);

        }
        else
        {
            isMoving = false;
            rbody.velocity = ChangeVelocityWithGravity(Vector3.zero);
        }

        //mySpeed = rbody.velocity;

    }

    private Vector3 ChangeVelocityWithGravity(Vector3 newVelocity)
    {
        float oldY = rbody.velocity.y;
        newVelocity.y = oldY;
        return newVelocity;
    }

    protected void RandomizeWanderDirection(Vector3 wallNormal)
    {
        if (wanderdirConfigureCooldown <= 0)
        {
            wanderDirection = (new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized + (wallNormal * 2)).normalized;
            wanderdirConfigureCooldown = 0.25f;
        }
    }
}
