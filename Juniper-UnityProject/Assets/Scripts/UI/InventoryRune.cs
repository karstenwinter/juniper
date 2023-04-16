using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InventoryRune : MonoBehaviour
{
    public string rune;
    public string location;
    public InventoryController inventoryController;

    public void Start()
    {
    }

    public void OnPointerEnter()
    {
        inventoryController.hoverInfo(this);
    }

    public void OnPointerExit()
    {
        inventoryController.hoverInfo(null);
    }

    public void OnPointerClick()
    {
        inventoryController.clickRune(this);
    }
}
