using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();

                if (instance == null)
                {
                    var g = new GameObject("Controller");
                    instance = g.AddComponent<T>();
                    //g.hideFlags = HideFlags.HideInHierarchy;
                }
            }

            return instance;
        }
    }

    private void Awake()
    {
        //DontDestroyOnLoad (gameObject);
        if (instance == null)
        {
            instance = this as T;
            //Debug.Log("Creating.." + gameObject.name);
        }
        else
        {
            if (instance != this) Destroy(gameObject);
        }
    }
}