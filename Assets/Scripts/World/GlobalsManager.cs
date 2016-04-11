using UnityEngine;
using System.Collections;

public class GlobalsManager : MonoBehaviour
{
    //----------------------//
    // Singleton Management //
    //----------------------//
    public static GlobalsManager instance = null;

	void Awake()
    {
        // If we have no set instance, set the instance to this
        if (instance == null)
        {
            instance = this;
        }

        // Otherwise, if we have an instance, and that instance is not this, destroy the game object
        // to prevent duplicates
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // Preserve this object
        DontDestroyOnLoad(gameObject);
    }

    //---------//
    // Globals //
    //---------//
    public int level = 1;
}
