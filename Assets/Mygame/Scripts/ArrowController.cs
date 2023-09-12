using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] float _destroyTime = 3f;
    [SerializeField] float _moveSpeed = 3f;
    Rigidbody _rb;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject , _destroyTime);
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = transform.forward * _moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
