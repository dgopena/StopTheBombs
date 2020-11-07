using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInitializable
{
    void Initialize();
}

public class SingletonResource<T> : ScriptableObject where T: ScriptableObject
{
    private static T _instance;
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                string resourceName = typeof(T).Name;
                _instance = Resources.Load<T>(resourceName);
                if (_instance == null)
                    Debug.LogWarning(resourceName + " not found in resources. name and type must match");
            }
            return _instance;
        }
    }
}

public class InitializedSingletonResource<T> : ScriptableObject where T : ScriptableObject, IInitializable
{
    private static T _instance;
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                string resourceName = typeof(T).Name;
                _instance = Resources.Load<T>(resourceName);
                if (_instance == null)
                    Debug.LogError(resourceName + " not found in resources. name and type must match");
                else
                    _instance.Initialize();
            }
            return _instance;
        }
    }
}