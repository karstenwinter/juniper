using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Globalization;

public class InteractableObject : ObjectFromTilemapBase
{
    public virtual bool GridBasedEnabled() { return true; }

    internal PlayerController playerController;
    public bool canBounceOff = true;
    public bool grantsManaOnHit = false;

    public virtual void protectionSpell(float damage)
    {
        hurt(damage);
    }

    public virtual void hurt(float damage)
    {
    }
}
