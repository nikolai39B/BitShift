using System;
using System.Collections.Generic;

/* 
    Ownership:

    Deleting a LevelWorld:
        - Delete each of the ChildPaths
            - Each path will clean up its nodes

    Deleting a LevelPath:
        - Remove parent reference from each of the ChildNodes
            - If that was the last parent, delete the node
        - Remove self from ParentWorld
*/

public class LevelWorld
{
    //-------------//
    // Constructor //
    //-------------//

    /// <summary>
    /// Instantiates a new level world with the given dimensions.
    /// </summary>
    /// <param name="rows">The number of rows in the world.</param>
    /// <param name="cols">The number of columns in the world.</param>
    public LevelWorld(int rows, int cols)
    {
        ChildPaths = new List<LevelPath>();
        ChildNodes = new LevelNode[rows, cols];
    }


    //------------//
    // References //
    //------------//

    public List<LevelPath> ChildPaths { get; private set; }
    public LevelNode[,] ChildNodes { get; private set; }


    //----------------//
    // Updating Paths //
    //----------------//

    /// <summary>
    /// Adds a new path to this world's list of paths.
    /// </summary>
    /// <param name="path">The path to add to the list.</param>
    public void AddPath(LevelPath path)
    {
        if (!ChildPaths.Contains(path))
        {
            ChildPaths.Add(path);
        }
    }

    /// <summary>
    /// Removes the given path from this world's list of paths.
    /// </summary>
    /// <param name="path">The path to remove from the list.</param>
    public void RemovePath(LevelPath path)
    {
        if (ChildPaths.Contains(path))
        {
            ChildPaths.Remove(path);
        }
    }


    //----------------//
    // Updating Nodes //
    //----------------//

    /// <summary>
    /// Adds a node to this world at the given location. Will throw an exception if this location is not empty.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="row">The row to place the node in.</param>
    /// <param name="col">The column to place the node in.</param>
    /// <exception cref="InvalidOperationException">Thrown if the given locaiton is not empty.</exception>
    public void AddNode(LevelNode node, int row, int col)
    {
        // Make sure this location is empty
        if (ChildNodes[row, col] != null)
        {
            throw new InvalidOperationException(string.Format(
                "Cannot add node to location ({0}, {1}); location already has node.", row, col));
        }

        // If we got here, we're good to proceed and add the node
        ChildNodes[row, col] = node;
    }

    /// <summary>
    /// Removes the node from this world at the given location.
    /// </summary>
    /// <param name="row">The row to remove the node from.</param>
    /// <param name="col">The column to remove the node from.</param>
    public void RemoveNode(int row, int col)
    {
        LevelNode nodeToRemove = ChildNodes[row, col];
        // Remove nodes from the node's parent path
    }
}

public enum Direction
{
    NONE,

    UP,
    RIGHT,
    DOWN,
    LEFT
}
