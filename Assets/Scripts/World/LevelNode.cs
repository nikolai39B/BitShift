using System.Collections.Generic;

public class LevelNode
{
    //-------------//
    // Constructor //
    //-------------//

    /// <summary>
    /// Instantiates a new level node with a single parent path.
    /// </summary>
    /// <param name="parentPath">The parent path for the node.</param>
    /// <param name="row">The row of the world to add the node to.</param>
    /// <param name="col">Thr column of the world to add the node to.</param>
    /// <param name="openSides">The sides of this node that are open, or null for no open sides.</param>
    public LevelNode(LevelPath parentPath, int row, int col, List<Direction> openSides = null)
    {
        ParentPath = parentPath;
        WorldRow = row;
        WorldColumn = col;

        // Set all sides to closed
        foreach (var side in directions)
        {
            bool sideIsOpen = openSides != null && openSides.Contains(side);
            UpdateOpenSideStatus(side, sideIsOpen);
        }

        // Insert the node into the structure
        ParentPath.AddNode(this, row, col);
    }


    //------------//
    // References //
    //------------//

    public LevelWorld ParentWorld { get { return ParentPath.ParentWorld; } }
    public LevelPath ParentPath { get; private set; }


    //---------------//
    // Updating Node //
    //---------------//

    /// <summary>
    /// Removes this node from its parent path and parent world.
    /// </summary>
    public void DeleteNode()
    {
        ParentPath.RemoveNode(this);
        ParentPath = null;
    }


    //----------//
    // Location //
    //----------//
    
    public int WorldRow { get; private set; }
    public int WorldColumn { get; private set; }


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


    //----------------//
    // Tail Node Flag //
    //----------------//

    /// <summary>
    /// Determines if this node is the tail node (the last node in order) of the parent path.
    /// </summary>
    /// <returns>True if this node is the tail node, false otherwise.</returns>
    public bool IsTailNodeOfPath()
    {
        int nodesInParentPath = ParentPath.ChildNodes.Count;
        return ParentPath.ChildNodes[nodesInParentPath - 1] == this;
    }
}