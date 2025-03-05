using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private bool dontDestroyOnLoad = false;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<T>();
            }
            if (instance == null)
            {
                GameObject obj = new GameObject(nameof(T));
                instance = obj.AddComponent<T>();
            }
            return instance;
        }
    }
    private static T instance;
    protected virtual void Awake()
    {
        CreateInstance();
    }
    protected virtual void CreateInstance()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}