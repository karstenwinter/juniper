using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RaycastController : MonoBehaviour
{
    public float len = 1f;
    public Vector2 dir = Vector2.right;
    public Color color = Color.red;
    public Text text;
    public bool circle = false;
    // float radius = 0.2f;
    
    private void Update()
    {
        Vector2 origin = transform.position;

        //LayerMask layerMask = LayerMask.GetMask("Platform");

        //RaycastHit2D collision = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) + origin, direction * distance, layerMask);
        RaycastHit2D collision;
        if(circle)
            collision = Physics2D.CircleCast(origin, len, dir, len);
        else
            collision = Physics2D.Raycast(origin, dir, len);
        //return collision.collider != null;
        //Debug.DrawLine(transform.position, Vector2.right, Color.yellow);
        //Debug.DrawLine(transform.position + distance * new Vector3(-transform.localScale.x, 0, 0), Vector2.right, Color.red);
        
        //var clingingInput = Input.GetAxis("Horizontal");
        //var sign1=Mathf.Sign(-transform.localScale.x);
        //var sign2=Mathf.Sign(clingingInput);

        //var grabbing = sign1 == sign2;
        drawString(collision.collider ? collision.collider.gameObject + "" : "none"); //"grabbing " + sign1+ ","+sign2+","+grabbing+"\n_isGrounded: "+_isGrounded);
    }
    
    void drawString(String s) {
        if(text != null)
            text.text = s;
    }
}
