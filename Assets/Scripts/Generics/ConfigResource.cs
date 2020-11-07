using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//singleton class for a configuration general object
public class ConfigResource<T> : ScriptableObject where T : Object
{
    private static T _instance;
    public static T instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<T>("Config/" + typeof(T).Name);
            return _instance;
        }
    }
}