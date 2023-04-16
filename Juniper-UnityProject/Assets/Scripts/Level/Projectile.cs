using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector2 direction;
    public int damageToPlayer;
    public float movingSpeed;
    public float destroyTime;
    Rigidbody2D _rigidbody;

    Vector2 newVelocity;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        string layerName = LayerMask.LayerToName(collider.gameObject.layer);

        if (layerName == "Player")
        {
            var playerController = collider.GetComponent<PlayerController>();
            playerController.hurt(damageToPlayer);
        }
    }

    public void trigger()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        newVelocity = direction.normalized * movingSpeed;
        _rigidbody.velocity = newVelocity;

        StartCoroutine(destroyCoroutine(destroyTime));
    }

    void Update()
    {
        if(_rigidbody != null)
        {
            _rigidbody.velocity = Global.isPaused ? new Vector2() : newVelocity;
        }
    }

    private IEnumerator destroyCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
