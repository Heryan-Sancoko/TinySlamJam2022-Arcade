using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProjectileBehaviour : MonoBehaviour
{
    private Rigidbody rbody;
    public float moveSpeed;
    [SerializeField]
    private float bulletLifetime;
    private float currentLifetime;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        rbody.velocity = transform.forward * moveSpeed;
        currentLifetime = bulletLifetime;
    }

    private void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0)
            gameObject.SetActive(false);
    }
}
