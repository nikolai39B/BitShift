using System.Collections.Generic;

public class LevelPath
{
    //-------------//
    // Constructor //
    //-------------//

    /// <summary>
    /// Instantiates a new level path in the given world.
    /// </summary>
    /// <param name="parentWorld">The path's parent world.</param>
    public LevelPath(LevelWorld parentWorld)
    {
        ParentWorld = parentWorld;
        ChildNodes = new List<LevelNode>();

        ParentWorld.AddPath(this);
    }


    //------------//
    // References //
    //------------//

    public LevelWorld ParentWorld { get; private set; }
    public List<LevelNode> ChildNodes { get; private set; }


    //---------------//
    // Updating Path //
    //---------------//

    /// <summary>
    /// Deletes all child nodes in this path.
    /// </summary>
    public void ClearPath()
    {
        // Delete each child node
        List<LevelNode> childNodesCopy = new List<LevelNode>(ChildNodes);
        foreach (var node in childNodesCopy)
        {
            node.DeleteNode();
        }

        ChildNodes = new List<LevelNode>();
    }

    /// <summary>
    /// Deletes all child nodes in this path and removes the path from the parent world.
    /// </summary>
    public void DeletePath()
    {
        // Remove all child nodes
        ClearPath();

        // Remove this path from the parent world
        ParentWorld.RemovePath(this);
        ParentWorld = null;
    }

    //----------------//
    // Updating Nodes //
    //----------------//

    /// <summary>
    /// Adds a node to this path and the parent world at the given location.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="row">The row to place the node in.</param>
    /// <param name="col">The column to place the node in.</param>
    public void AddNode(LevelNode node, int row, int col)
    {
        if (!ChildNodes.Contains(node))
        {
            ParentWorld.AddNode(node, row, col);
            ChildNodes.Add(node);
        }
        
    }

    /// <summary>
    /// Removes the given node from this world.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemoveNode(LevelNode node)
    {
        if (ChildNodes.Contains(node))
        {
            ParentWorld.RemoveNode(node);
            ChildNodes.Remove(node);
        }
    }
}
