using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Index
{    
    private static Index<BasicTag> basicTagIndex = new Index<BasicTag>();
    public static Index<BasicTag> BasicTagIndex { get { return basicTagIndex; } }
}

public class Index<T>
{
    private Dictionary<T, List<GameObject>> TagIndex = new Dictionary<T, List<GameObject>>();

    /// <summary>
    /// Adds the provided game object to the index under the given tag. The game object can then be indentified by other scripts through the index.
    /// </summary>
    /// <param name="tag">The tag with which to index the object.</param>
    /// <param name="obj">The game object to index.</param>
    public void AddToIndex(T tag, GameObject obj)
    {
        if (!TagIndex.ContainsKey(tag))
        {
            TagIndex[tag] = new List<GameObject>();
        }
        TagIndex[tag].Add(obj);
    }

    /// <summary>
    /// Removes the provided game object with a given tag from the index.
    /// </summary>
    /// <param name="tag">The tag being used to index the object.</param>
    /// <param name="obj">The game object to remove.</param>
    public void RemoveFromIndex(T tag, GameObject obj)
    {
        if (TagIndex.ContainsKey(tag) && TagIndex[tag].Contains(obj))
        {
            TagIndex[tag].Remove(obj);
        }
    }

    /// <summary>
    /// Returns a list of game objects with the given tag.
    /// </summary>
    /// <param name="tag">The tag to signifiy which game objects to return.</param>
    /// <returns></returns>
    public List<GameObject> GetObjectsWithTag(T tag)
    {
        if (TagIndex.ContainsKey(tag))
        {
            return TagIndex[tag];
        }

        return new List<GameObject>();
    }

    /// <summary>
    /// Returns a game object with the given tag.
    /// </summary>
    /// <param name="tag">The tag to signifiy which game object to return.</param>
    /// <returns></returns>
    public GameObject GetObjectWithTag(T tag)
    {
        List<GameObject> objs = GetObjectsWithTag(tag);

        if (objs.Count > 0)
        {
            return objs.First();
        }

        return null;
    }
}
