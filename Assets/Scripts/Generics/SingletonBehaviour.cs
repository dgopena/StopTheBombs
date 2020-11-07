using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a monobehavior based singleton base class
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T _instance;
    public static T instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<T>();
            return _instance;
        }
    }
}
