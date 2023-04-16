using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class InputLayout {
    public string cancel = "Cancel";
    public string attack = "Btn0"; // []
    public string jump = "Btn1"; // X
    public string magic = "Btn2"; // O
    public string dash = "Btn7"; // RT
    public string map = "Btn4"; // LB
    
    public string horizontal = "Horizontal";
    public string vertical = "Vertical";

    public string attackAlt = "Mouse0"; // LM
    public string dashAlt = "Mouse1"; // RM
    public string horizontalAlt = null;
    public string verticalAlt = null;
    
    public static InputLayout keyboard = new InputLayout();
    public static InputLayout touchpad = new InputLayout {
        attackAlt = null,
        dashAlt = null
    };

    public static InputLayout dualshockPC = new InputLayout {
        horizontalAlt = "Axis7",
        verticalAlt = "Axis8"
    };
    public static InputLayout dualshockAndroid = new InputLayout {
        attack = "Btn2",
        jump = "Btn0",
        magic = "Btn1",
        horizontalAlt = "Axis5",
        verticalAlt = "Axis6"
    };

    public static InputLayout xBox = new InputLayout {
        jump = "Btn0",
        magic = "Btn1",
        attack = "Btn2",
        dash = "Btn3",
        horizontalAlt = "Axis6",
        verticalAlt = "Axis7"
    };
}
