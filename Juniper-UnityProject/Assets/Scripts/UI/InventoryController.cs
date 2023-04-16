using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class InventoryController : MonoBehaviour
{
    public AudioSource clickSound, failSound;
    public SpriteRenderer fade;
    public Text text, word1, word2, word3;
    public GameObject equipSlotsLeft, equipSlotsRight, equipSlotsStatic, availableLeft, availableRight, availableStatic;
    public GameObject buttonPrefab;

    // TODO player state
    public string[] runesLeft = new string[3];
    public string[] runesRight = new string[3];
    public string[] runesStatic = new string[3];
    public string[] runesAvailableLeft = new string[4];
    public string[] runesAvailableRight = new string[4];
    public string[] runesAvailableStatic = new string[4];

    string[] locationNames = new[] { "Trigger", "Effect", "Static", "AvailableTrigger", "AvailableEffect", "AvailableStatic" };
    string[] connectorNames = new[] { "Right", "Left", "Static", "Right", "Left", "Static" };
    string[][] state = new string[0][];
    GameObject[] containers = new GameObject[0];
    Text[] words = new Text[0];
    string selectedRune = "";

    public InventoryController()
    {
    }

    public void Start()
    {
        Load();
        Translations.Apply(Global.settings.language);
    }

    [ContextMenu("Load")]
    public void Load()
    {
        state = new[] { runesLeft, runesRight, runesStatic, runesAvailableLeft, runesAvailableRight, runesAvailableStatic };
        containers = new[] { equipSlotsLeft, equipSlotsRight, equipSlotsStatic, availableLeft, availableRight, availableStatic };
        words = new[] { word1, word2, word3 };

        var i = 0;

        foreach (var container in containers)
        {
            var currentState = state[i];
            var currentConnectorName = connectorNames[i];
            var locationName = locationNames[i];
            i++;

            foreach (Transform item in container.transform.Cast<Transform>().ToList())
            {
#if UNITY_EDITOR
               GameObject.DestroyImmediate(item.gameObject);
#else
            GameObject.Destroy(item.gameObject);
#endif
            }

            foreach(var st in currentState)
            {
                var rune = Array.Find(Caches.RuneData, x => st == x.rune);
                if (rune != null)
                {
                    var currentButton = Instantiate(buttonPrefab, container.transform);
                    var invRune = currentButton.GetComponent<InventoryRune>();
                    invRune.rune = rune.rune;
                    if (rune.rune == selectedRune || selectedRune == "")
                        EventSystem.current.SetSelectedGameObject(currentButton.gameObject);
                    
                    selectedRune = selectedRune == "" ? " " : selectedRune;

                    invRune.location = locationName;

                    currentButton.gameObject.name = rune.name;
                    currentButton.GetComponentInChildren<Text>().text = Translations.For(rune.name);
                    currentButton.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("DownloadedObjectImages/" + rune.rune);

                    for (int n = 1; n <= 3; n++)
                    {
                        var x = currentButton.transform.Find(currentConnectorName + n);
                        if(x != null)
                            x.gameObject.SetActive(true);
                    }
                }
            }
        }
        RecalcWords();
    }

    public void hoverInfo(InventoryRune rune)
    {
        text.text = rune == null ? Translations.For("SelectForDetails") : detail(rune);
    }

    string detail(InventoryRune rune)
    {
        var runeData = Array.Find(Caches.RuneData, x => x.name == rune.name);
        var l = "\n___\n" + Translations.For("SymbolOf", rune.rune, Translations.For(rune.name), Translations.For(runeData.translation));
        if (runeData == null)
            return "MISSING DATA" + l;

        return Translations.For(runeData.staticEffect)
            + Translations.For(runeData.triggeredEffect)
            + Translations.For(runeData.condition)
            + l;
    }

    public void clickRune(InventoryRune rune)
    {
        var newLoc = rune.location.Contains("Available") ? rune.location.Replace("Available", "") : "Available" + rune.location;

        var oldLocIndex = Array.IndexOf(locationNames, rune.location);
        var newLocIndex = Array.IndexOf(locationNames, newLoc);
        if (IsFull(state[newLocIndex]))
        {
            failSound.Play();
        }
        else
        {
            selectedRune = rune.rune;
            Remove(state[oldLocIndex], rune.rune);
            Add(state[newLocIndex], rune.rune);

            clickSound.Play();

            Load();

        }
    }

    void RecalcWords()
    {
        for (int i = 0; i < words.Length; i++)
        {
            var word = "";
            var left = runesLeft[i];
            var right = runesRight[i];
            var mod = runesStatic[i];

            if ((left == "") != (right == ""))
                word = Translations.For("Incomplete");

            if (left != "" && right != "")
            {
                var runeDataLeft = Array.Find(Caches.RuneData, x => x.rune == left);
                var runeDataRight = Array.Find(Caches.RuneData, x => x.rune == right);
                var runeDataMod = Array.Find(Caches.RuneData, x => x.rune == mod);

                word = Translations.For(runeDataLeft?.condition).Trim() + " " + Translations.For(runeDataRight?.triggeredEffect).Trim();
            }
            words[i].text = word;
        }
    }

    bool IsFull(string[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == "")
                return false;
        }
        return true;
    }

    void Remove(string[] array, string item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == item)
            {
                array[i] = "";
                for(int afterI = i; afterI < array.Length; afterI++)
                {
                    if(array[afterI] != "")
                    {
                        array[afterI - 1] = array[afterI];
                        array[afterI] = "";
                    }
                }
                break;
            }
        }
    }

    void Add(string[] array, string item)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == "")
            {
                array[i] = item;
                break;
            }
        }
    }
}
