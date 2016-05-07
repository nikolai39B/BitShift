using System;
using System.Collections.Generic;

/* 
    How to work in the Level World/Path/Node environment:

    1) Creating a world
        a) Construct a new world using the LevelWorld constructor
            1) Pass in the number of rows and columns in the world

    2) Adding paths
        a) Construct a new path using the LevelPath constructor
            1) Pass in the parent world
            2) This will call the add path method in the parent world

    3) Adding nodes
        a) Determine the row and column to place the node
            1) Make sure this location is empty, or you will have to handle an InvalidOperationException
        b) Construct a new node using the LevelNode constructor
            1) Pass in the parent path
            2) Pass in the row and column to place it at in the parent world
            3) Optinally pass in which sides are currently open
            4) This will call the add node method in the parent path and world

    4) Removing a node
        a) Call the DeleteNode() method on the node you wish to delete
            1) This will remove the node from the parent path and world

    5) Clear / Removing a path
        a) Clearing a path without removing it from the world
            1) Call the ClearPath() method on the path you wish to clear
            2) This will delete all child nodes in the path
        b) Deleteing a path
            1) Call the DeletePath() method on the path you wish to delete
            2) This will clear the path then remove it from the world

    6) Clearing a world
        a) Call the ClearWorld() method on the world you wish to clear
            1) This will delete all child paths and nodes
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
    // Updating World //
    //----------------//

    /// <summary>
    /// Deletes all child paths and nodes in the world.
    /// </summary>
    public void ClearWorld()
    {
        // Delete each child path (which will also delete each child node)
        List<LevelPath> childPathsCopy = new List<LevelPath>(ChildPaths);
        foreach (var path in childPathsCopy)
        {
            path.ClearPath();
        }

        ChildPaths = new List<LevelPath>();
        ChildNodes = new LevelNode[RowsInWorld, ColumnsInWorld];
    }


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
    /// <exception cref="InvalidOperationException">Thrown if the given location is not empty.</exception>
    public void AddNode(LevelNode node, int row, int col)
    {
        // Make sure this location is empty
        if (ChildNodes[row, col] != null)
        {
            throw new InvalidOperationException(string.Format(
                "Cannot add node to location ({0}, {1}); location already has a node.", row, col));
        }

        // If we got here, we're good to proceed and add the node
        ChildNodes[row, col] = node;
    }

    /// <summary>
    /// Removes the given node from this world. Will throw an exception if the node's store coordinates do not match its actual coordinates.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown if the given node's location is occupied by a different node.</exception>
    public void RemoveNode(LevelNode node)
    {
        // Check to make sure this node is registered at the correct location
        if (ChildNodes[node.WorldRow, node.WorldColumn] == node)
        {
            // If it is, remove it
            ChildNodes[node.WorldRow, node.WorldColumn] = null;
        }

        // If it's not, throw an exception
        else
        {
            throw new InvalidOperationException(string.Format(
                "Cannot remove node from location({0}, {1}); this location is occupied by a different node.", node.WorldRow, node.WorldColumn));
        }
    }


    //------------------//
    // World Dimensions //
    //------------------//

    public int RowsInWorld { get { return ChildNodes.GetLength(0); } }
    public int ColumnsInWorld { get { return ChildNodes.GetLength(1); } }
}

public enum Direction
{
    NONE,

    UP,
    RIGHT,
    DOWN,
    LEFT
}
