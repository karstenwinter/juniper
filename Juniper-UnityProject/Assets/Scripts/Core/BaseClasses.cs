using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Reflection;
using System.Globalization;

public interface IModal {
    void OpenModal(string question, Action onYes, Action onNo);
}

public class ObjectFromTilemapBase : MonoBehaviour
{
    public new SpriteRenderer renderer;
    protected TableData data;

    public virtual void InitFromData(TableData data)
    {
        this.data = data;
    }
    public virtual void InitFromName(string name)
    {
        gameObject.name = name;
    }

    public virtual void InitFromKeyValues(string dataString)
    {
        var data = Array.ConvertAll(dataString.Split(','), x => {
            var parts = x.Trim().Split(':');
            return new KeyValuePair<string, string>(parts[0].Trim(), parts.Length > 1 ? parts[1].Trim() : "");
        });
        Debug.Log(String.Join(", ", data));
        foreach(var pair in data) 
        {
            var res = lookup(pair.Key);
            if(res.Key != null && pair.Value != null)
                setValue(res, pair.Value);
        }
    }

    void setValue(KeyValuePair<Type, FieldInfo> pair, string value)
    {
        try {
            var field = pair.Value;
            var converted = System.Convert.ChangeType(value, field.FieldType, CultureInfo.InvariantCulture);
            Debug.Log("converted val " + converted + " (" + converted.GetType() + ")");
            
            var type = pair.Key;
            var comp = GetComponent(type);
            Debug.Log("Comp " + comp);
            if(comp != null) 
            {
                field.SetValue(comp, converted);
            }

        }
        catch (Exception e) 
        {
            Debug.Log("set value failed: " + e);
        }
    }

    static KeyValuePair<Type, FieldInfo> lookup(string key) 
    {
        Type t = null;
        FieldInfo field = null;
        KeyValuePair<Type, FieldInfo> lookupRes;
        if(Caches.fieldLookup.TryGetValue(key, out lookupRes)) {
            return lookupRes;
        }

        var partsKey = key.Split('.');
        if(partsKey.Length > 1) 
        {
            var typeName = partsKey[0].Trim();
            var fieldName = partsKey[1].Trim();

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (ass.FullName.StartsWith("System."))
                    continue;
                t = ass.GetType(typeName);
                if (t != null)
                    break;
                t = ass.GetType("UnityEngine."+typeName);
                if (t != null)
                    break;
            }

            if(t != null) 
            {
                field = t.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            }

            if(Global.isDebug) 
            {
                Global.LogDebug("Reflection: " + fieldName + "=>" + field + " from " + t + " (from " + typeName + ")");
            }
            lookupRes = new KeyValuePair<Type, FieldInfo>(t, field);
            Caches.fieldLookup[key] = lookupRes;
            return lookupRes;
        }
        return default(KeyValuePair<Type, FieldInfo>);
    }
}


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

public abstract class State<T>
{
    public abstract bool CheckValid(T enemyController);
    public abstract void Execute(T enemyController);
}
