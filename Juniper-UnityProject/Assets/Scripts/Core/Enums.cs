using System;

public enum MainMenuButtons {
    StartGame, Options, Community, Exit
}

public enum OptionsMenu {
    Profile, Difficulty, Language, Input
}

public enum PauseMenu {
    Continue, Save, Load, Blur, AntiAliasing, UIScale, ToTitle
}

public enum Quality {
    Off, Low, Mid, High
}

public enum OffOn {
    Off, On
}

public enum Scaling {
    Tiny, Small, Normal, Big, Huge
}

public enum Language {
    EN, DE, ES, RU, JA, CN
}

public enum GameProfile {
	First, Second, Third, Fourth, Test
}

public enum InputType {
    KeyboardGamepad, Touchpad, TouchpadInvisible
    //Keyboard, DualshockPC, DualshockAndroid, XBox, Touchpad, TouchpadInvisble, Custom
}

public enum Profile {
    First, Second, Third, Fourth
}

public enum Difficulty {
    Easy, Normal, Hard
}

public enum AudioVolume {
   	Off, Low, Mid, High
}

public enum InputThreshold {
   	Off, VeryLow, Low, Mid, High
}

public enum WorldArea {
    TheSewers,                      TheForest, 
    MossyCaverns,                   UpperCave,
                       WesternCave, LowerCave,       EasternCaves, StonyCaves, 
                                                     UpperJuction, 
    ThePit,            DarkGrounds, CrystalJunction, SandDepths,
}

public enum UpgradeType {
    SelectUpgrade = 0,
    Needle = 1,
    Dash = 2,
    Heal = 3,
    DoubleJump = 4,
    MagicMissile = 5,
    SuperDash = 6,
    Map = 7
}