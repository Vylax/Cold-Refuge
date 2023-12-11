using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static GameManager instance;

    // Property to access the singleton instance
    public static GameManager Instance
    {
        get
        {
            // If the instance is null, try to find an existing GameManager in the scene
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                // If no GameManager is found, create a new GameObject and add the GameManager script to it
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject("GameManager");
                    instance = singletonObject.AddComponent<GameManager>();
                }
            }

            // Return the instance
            return instance;
        }
    }

    private void Awake()
    {
        // Ensure there is only one instance of GameManager in the scene
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}
