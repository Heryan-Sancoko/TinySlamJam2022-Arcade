using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rbody;
    public float moveSpeed;

    // Start is called before the first frame update
    private void OnEnable()
    {
        rbody.velocity = transform.forward * moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
