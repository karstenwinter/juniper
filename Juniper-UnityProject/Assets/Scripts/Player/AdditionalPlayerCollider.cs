using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class AdditionalPlayerCollider : MonoBehaviour
{
    public bool currentValue;

    public PlayerController playerController;
    public bool isLeft;
    public bool isBottom;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag != "Platform")
            return;

        currentValue = true;
        if (playerController != null)
            if (isBottom)
                playerController.hasContactBottom = currentValue;
            else if (isLeft)
                playerController.hasContactLeft = currentValue;
            else
                playerController.hasContactRight = currentValue;
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.tag != "Platform")
            return;

        currentValue = false;
        if (playerController != null)
            if (isBottom)
                playerController.hasContactBottom = currentValue;
            else if (isLeft)
                playerController.hasContactLeft = currentValue;
            else
                playerController.hasContactRight = currentValue;
    }

    /* public void OnCollisionEnter2D(Collision2D collision)
     {
         Debug.Log("OnCollisionEnter2D" + collision + "," + collision.collider);
         if (collision.collider.tag != "Platform")
             return;

         currentValue = true;
         if (isLeft)
             playerController.hasContactLeft = currentValue;
         else
             playerController.hasContactRight = currentValue;
     }
     public void OnCollisionExit2D(Collision2D collision)
     {
         Debug.Log("OnCollisionExit2D" + collision + "," + collision.collider);
         if (collision.collider.tag != "Platform")
             return;

         currentValue = false;

         if (isLeft)
             playerController.hasContactLeft = currentValue;
         else
             playerController.hasContactRight = currentValue;
     }*/
}