using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHack : MonoBehaviour
{
    public CustomOnScreenButton button;

    public void TriggerButtonPressed()
    {
        button.TriggerButtonPressed();
    }
    public void ResetButtonPressed()
    {
        button.ResetButtonPressed();
    }
}
