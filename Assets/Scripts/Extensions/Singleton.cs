using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected bool ShouldBeDestroyed { get; private set; }

    private static T instance;

    public static T Instance{
        get{
            if (instance == null){
                return default;
            }
            return instance;
        }
        private set { instance = value; }
    }

    [SerializeField] protected bool dontDestroyOnLoad, ignoreErrorWhenDuplicatedInstance;

    protected virtual void Awake()
    {
        T thisInstance = this as T;

        if (instance && instance != thisInstance){
            ShouldBeDestroyed = true;

            Destroy(thisInstance);
            if (!ignoreErrorWhenDuplicatedInstance)
                Debug.LogError("Duplicate Singleton instance found of type: " + thisInstance.ToString() +
                               " destroying instance.");

            return;
        }

        instance = thisInstance;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }
}

