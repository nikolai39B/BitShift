using System.Collections.Generic;

public class LevelNode
{
    //-------------//
    // Constructor //
    //-------------//

    /// <summary>
    /// Instantiates a new level node with a single parent path.
    /// </summary>
    /// <param name="parentWorld">The parent world for the node.</param>
    /// <param name="parentPath">The parent path for the node.</param>
    public LevelNode(LevelWorld parentWorld, LevelPath parentPath, List<Direction> openSides)
        : this(parentWorld, new List<LevelPath> { parentPath }, openSides)
    {
    }

    /// <summary>
    /// Instantiates a new level node with multiple parent paths.
    /// </summary>
    /// <param name="parentWorld">The parent world for the node.</param>
    /// <param name="parentPaths">A list of the parent paths for the node.</param>
    public LevelNode(LevelWorld parentWorld, List<LevelPath> parentPaths, List<Direction> openSides)
    {
        ParentWorld = parentWorld;
        ParentPaths = ParentPaths;

        // Set all sides to closed
        foreach (var side in directions)
        {
            UpdateOpenSideStatus(side, openSides.Contains(side));
        }
    }


    //------------//
    // References //
    //------------//

    public LevelWorld ParentWorld { get; private set; }
    public List<LevelPath> ParentPaths { get; private set; }


    //----------------//
    // Updating Paths //
    //----------------//

    


    //----------//
    // Location //
    //----------//

    private Tuple<int, int> worldCoordinates = null;
    public Tuple<int, int> WorldCoordinates
    {        
        get
        {
            // Lazy initialization
            // Note that this value should never change in the world
            if (worldCoordinates == null)
            {
                worldCoordinates = GetWorldCoordinatesOfNode();
            }

            return worldCoordinates;
        }
    }

    /// <summary>
    /// Gets the world array row and column of this node.
    /// </summary>
    /// <returns>The node's coordinates.</returns>
    private Tuple<int, int> GetWorldCoordinatesOfNode()
    {
        int row = -1;
        int col = -1;

        // Loop through all the elements in the world
        for (int rr = 0; rr < ParentWorld.ChildNodes.GetLength(0); rr++)
        {
            for (int cc = 0; cc < ParentWorld.ChildNodes.GetLength(1); cc++)
            {
                // If we've found ourself, then set the row and col
                if (ParentWorld.ChildNodes[rr, cc] == this)
                {
                    row = rr;
                    col = cc;
                }
            }
        }

        return new Tuple<int, int>(row, col);
    }


    //------------//
    // Open Sides //
    //------------//

    // Public accessor for the node's open sides
    public List<Direction> OpenSides
    {
        get
        {
            // Instantiate direction lists
            List<Direction> openSidesList = new List<Direction>();

            // Loop through each direction
            foreach (var dir in directions)
            {
                // If this side is open in the dictionary, add it to the list
                if (openSides[dir])
                {
                    openSidesList.Add(dir);
                }
            }

            return openSidesList;
        }
    }

    // Private list of directions for iterating through
    private List<Direction> directions = new List<Direction>()
    {
        Direction.UP,
        Direction.RIGHT,
        Direction.DOWN,
        Direction.LEFT
    };

    // Private container for the node's open sides
    private Dictionary<Direction, bool> openSides = new Dictionary<Direction, bool>();

    /// <summary>
    /// Updates the open status for the given side
    /// </summary>
    /// <param name="side"></param>
    /// <param name="isOpen"></param>
    public void UpdateOpenSideStatus(Direction side, bool isOpen)
    {
        openSides[side] = isOpen;
    }


    //-----------------//
    // Tail Node Flags //
    //-----------------//

    /// <summary>
    /// Determines if this node is the tail node (the last node in order) of any path.
    /// </summary>
    /// <returns>True if this node is a tail node, false otherwise.</returns>
    public bool IsTailNodeOfSomePath()
    {
        // Loop through all parent paths, and check if we're the tail node of any of them
        foreach (var path in ParentPaths)
        {
            if (IsTailNodeOfSpecificPath(path))
            {
                return true;
            }
        }

        // If we get here, then we're not a tail node
        return false;
    }

    /// <summary>
    /// Determines if this node is the tail node (the last node in order) in the given path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <returns>True if this node is the tail node, false otherwise.</returns>
    public bool IsTailNodeOfSpecificPath(LevelPath path)
    {
        int nodesInParentPath = path.ChildNodes.Count;
        return path.ChildNodes[nodesInParentPath - 1] == this;
    }
}