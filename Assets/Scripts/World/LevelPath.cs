using System.Collections.Generic;

public class LevelPath
{
    //-------------//
    // Constructor //
    //-------------//

    /// <summary>
    /// Instantiates a new level path in the given world with the given nodes.
    /// </summary>
    /// <param name="parentWorld">The path's parent world.</param>
    /// <param name="existingNodes">The path's starting nodes (or null for no starting nodes).</param>
    public LevelPath(LevelWorld parentWorld, List<LevelNode> existingNodes = null)
    {
        ParentWorld = parentWorld;
        ChildNodes = existingNodes == null ? new List<LevelNode>() : existingNodes;
    }


    //------------//
    // References //
    //------------//

    public LevelWorld ParentWorld { get; private set; }
    public List<LevelNode> ChildNodes { get; private set; }

    //----------------//
    // Path Splitting //
    //----------------//

    /// <summary>
    /// Instantiates a new path with this path's nodes and world.
    /// </summary>
    /// <returns>The new path.</returns>
    public LevelPath CreateDuplicatePath()
    {
        LevelPath newPath = new LevelPath(ParentWorld, ChildNodes);

        // Update references
        ParentWorld.AddPath(newPath);
        foreach (var node in ChildNodes)
        {
            node
        }

        return newPath;
    }
}
