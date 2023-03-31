using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float speed = 0.4f;
    public float jumpImpulse = 10;
    Rigidbody2D _rigidbody;
    new Rigidbody2D rigidbody { get { return _rigidbody == null ? _rigidbody = GetComponent<Rigidbody2D>() : _rigidbody; } }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetButtonDown("Fire1") ? Input.GetAxis("Vertical") : 0,
            0
        ) * speed;
        if (Input.GetButtonDown("Jump"))
        {
            rigidbody.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
        }
    }
}
