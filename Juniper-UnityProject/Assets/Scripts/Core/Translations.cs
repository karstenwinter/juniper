using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DictMap : Dictionary<Language, Dictionary<string, string>>{
	public DictMap() {
		foreach (Language language in Enum.GetValues(typeof(Language)))
		{
			this[language] = new Dictionary<string, string>();
		}	
	}

    void AddEntry(Language language, string key, string value) {
        if(!ContainsKey(language)) {
            this[language] = new Dictionary<string, string>();
        }
        this[language][key] = value;
    }

    public void Add(Language language, object obj, string value) {
        AddEntry(language, "" + obj, value);
    }
}

public class Translations {
	// public static Dictionary<string, object> allEnumValues = getAllValues();
	// private static Dictionary<string, object> getAllValues() {
	// 	var x = new Dictionary<string, object>();
	// 	foreach (Type t in new []{
	// 			typeof(OptionsMenu), typeof(MainMenuButtons), typeof(PauseMenu),
	// 			typeof(InputType), typeof(Profile), typeof(Difficulty), typeof(Areas),
	// 			typeof(AudioVolume)
	// 		}) {
	// 		foreach(var item in Enum.GetValues(t)) {
	// 			x[item.ToString()] = item;
	// 		}
	// 	}
	// 	return x;
	// }

	
	public static string For(string key, params object[] arr) {
		
		switch(key) {
			case null: return "";
			case "DE": return "Deutsch";
			case "ES" : return "Español";
			case "RU": return "Русский";
			case "EN" : return "English";
            case "JA": return "日本語";
            case "CN": return "官話";
        }

		var lang = Global.settings.language;
		Dictionary<string, string> forLang;
		if(Caches.Translations.TryGetValue(lang, out forLang))
		{
			if(key != null)
			{
				string res;
				if(forLang.TryGetValue(key, out res))
				{
                    // Debug.Log("key: " + key + " => " + res);
                    if (res.Contains("{"))
                    {
                        try
                        {
                            res = String.Format(res, arr);
                        } catch (Exception e) { Global.HandleError(e);  }
                    }
						
					return res.Replace("\\n", "\n").Replace("\\t", "\t");
				}
				else
				{
					// var value = allEnumValues[key];
					// Debug.LogError("{ Language." + lang + ", " + value.GetType().Name + "." + key + ", \"" + lang+" "+key+"\" },");
					Global.LogDebug("Missing: " + key + " in " + lang);
				}	
			}
		}
		return key;
	}
	
	public static void Apply(Language lang) {
		//var keys = allEnumValues.Keys;
		var currentLang = Caches.Translations[lang];
		var objMap = new Dictionary<string, object>();
		foreach(var g in Resources.FindObjectsOfTypeAll<GameObject>()) {
			if(currentLang.ContainsKey(g.name))
			{
				object old;
				if(objMap.TryGetValue(g.name, out old))
                {
					var arr = old as GameObject[];
					if(arr == null)
					{ arr = new GameObject[] { (GameObject) old };  }
					Array.Resize(ref arr, arr.Length + 1);
					arr[arr.Length - 1] = g;
					objMap[g.name] = arr;
				}
				else
					objMap[g.name] = g;
			}
		}
		
		foreach(var key in objMap.Keys) {
			var found = objMap[key];
			var str = currentLang[key].Replace("\\n", "\n").Replace("\\t", "\t");
			var asArr = found as GameObject[];
			
			if(asArr != null)
            {
                foreach (var item in asArr)
                {
					var text =
					item.GetComponent<Button>()?.GetComponentInChildren<Text>()
					 ?? item.GetComponent<Text>();
					if (text != null)
					{
						text.text = str;
					}
				}
			} else
            {
				var go = (GameObject) found;
				var text =
					go.GetComponent<Button>()?.GetComponentInChildren<Text>()
					 ?? go.GetComponent<Text>();
				if (text != null)
				{
					text.text = str;
				}
			}
		}
	}
}
