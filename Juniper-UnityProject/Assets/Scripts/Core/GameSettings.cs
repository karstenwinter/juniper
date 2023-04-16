using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;


[Serializable]
public class GameSettings
{
    public GameProfile profile = GameProfile.First;
    public Language language = Language.EN;

    public InputType input =
#if UNITY_ANDROID
                        InputType.Touchpad
#else
                        InputType.Touchpad // InputType.KeyboardGamepad
#endif
                 ;

    public AudioVolume music = AudioVolume.Mid;
    public AudioVolume sound = AudioVolume.Mid;

    public Quality blur = Quality.Mid;
    public OffOn antiAliasing = OffOn.Off;
    public OffOn debug = OffOn.Off;
    public OffOn mobileAutoFire = OffOn.Off;
    public OffOn autoFireButtons = OffOn.Off;
    public OffOn climbWithHorizontal = OffOn.On;

    public InputThreshold inputThreshold = InputThreshold.Low;
}