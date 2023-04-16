using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DummyCharacter : MonoBehaviour
{
    public float speed = 1;

    internal float Horizontal, Vertical;
    internal bool Map, Jump, Attack, Magic, Dash, Pause;

    public void OnMap(InputAction.CallbackContext context)
    {
        Map = context.action.inProgress;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump = context.action.inProgress;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        Attack = context.action.inProgress;
    }

    public void OnMagic(InputAction.CallbackContext context)
    {
        Magic = context.action.inProgress;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        Dash = context.action.inProgress;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
            Pause = true;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("c" + context);
        var v = context.action.ReadValue<Vector2>();
        Horizontal = v.x;
        Vertical = v.y;

    }
    Vector3 start;
    void Start()
    {
        start = transform.position;
    }
    // Update is called once per frame
    void Update()
    {
        var dir = new Vector3(Horizontal, Vertical) * speed;
        var text = "dir " + dir + "J" + Jump + " D" + Dash;
        Debug.Log(text);
        GameObject.Find("AnyKey").GetComponent<Text>().text = text;
        transform.position += dir;
        if (Jump)
        {
            transform.position = start;
        }


        transform.localScale = Vector3.one * (Dash ? 2 : 1);

    }
}
