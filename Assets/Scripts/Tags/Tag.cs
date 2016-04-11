using UnityEngine;
using System.Collections;

public class Tag : MonoBehaviour
{
    public BasicTag basicTag;

    void Awake()
    {
        Index.BasicTagIndex.AddToIndex(basicTag, this.gameObject);
    }

    void OnDestroy()
    {
        Index.BasicTagIndex.RemoveFromIndex(basicTag, this.gameObject);
    }
}

public enum BasicTag
{
    // Default
    NONE = 0,

    // General
    CAMERA = 100,
    GAME_MANAGER = 101,
    UI = 102,
    GLOBALS_MANAGER = 103,
    
    // Actors
    PLAYER = 200,
    ENEMY = 201,

    // Boundary Objects
    WALL = 300,
}
