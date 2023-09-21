using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    [SerializeField] GameObject _destroyEffect;
    [SerializeField] float _gravity = 0.2f;
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
    private void FixedUpdate()
    {
        _rb.velocity -= new Vector3(0,_gravity,0) * Time.deltaTime;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "Field")
        {
            Instantiate(_destroyEffect , transform.position , transform.rotation ,null);
            Destroy(gameObject);
        }
    }
}
