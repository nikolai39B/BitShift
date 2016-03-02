using System;
using System.Collections.Generic;
using System.Linq;

public enum Direction
{
    NONE,

    UP,
    RIGHT,
    DOWN,
    LEFT
}

public enum PathGenerationStatus
{
    NONE,

    ADDED_NODE,
    REMOVED_NODE,
    PATH_FINISHED,
    PATH_FAILED
}

public class PathNode
{
    /// <summary>
    /// Instantiates a new PathNode object.
    /// </summary>
    /// <param name="parentPath">The owning Path instance.</param>
    /// <param name="previousNode">The previous node in the path.</param>
    /// <param name="nextNode">The next node in the path.</param>
    /// <param name="worldRow">The row in the world that this node occupies.</param>
    /// <param name="worldCol">The column in the world that this node occupies.</param>
    /// <param name="nodeRole">The role this node plays.</param>
    public PathNode(Path parentPath, int worldRow, int worldCol, Role nodeRole = Role.NORMAL)
    {
        ParentPath = parentPath;
        WorldRow = worldRow;
        WorldCol = worldCol;
        NodeRole = nodeRole;
    }

    //------//
    // Role //
    //------//
    public enum Role
    {
        NORMAL,     // The standard role for a node
        START,      // The first node in the path
        END,        // The last node in a complete path
        TEMPORARY,  // A node that is just a temporary placeholder
        BLOCK,      // A node that should never be passed through
    }
    public Role NodeRole { get; set; }

    //------------//
    // Directions //
    //------------//
    /// <summary>
    /// Gets the direction to the given node. If the given node is null or not directly vertical or horizontal, will return NONE.
    /// </summary>
    /// <param name="node">The node to get the direction to.</param>
    private Direction GetDirectionToNode(PathNode node)
    {
        // Handle a null node
        if (node == null)
        {
            return Direction.NONE;
        }

        // Handle a node in the same location, or a node diagonal to this one
        if ((node.WorldRow == this.WorldRow && node.WorldCol == this.WorldCol) ||
            (node.WorldRow != this.WorldRow && node.WorldCol != this.WorldCol))
        {
            return Direction.NONE;
        }

        // Handle remaining cases (horizontal or vertical)
        if (node.WorldRow < this.WorldRow)
        {
            return Direction.UP;
        }
        else if (node.WorldCol > this.WorldCol)
        {
            return Direction.RIGHT;
        }
        else if (node.WorldRow > this.WorldRow)
        {
            return Direction.DOWN;
        }
        else
        {
            return Direction.LEFT;
        }
    }
    public Direction DirToPreviousNode
    {
        get { return GetDirectionToNode(PreviousNode); }
    }
    public Direction DirToNextNode
    {
        get { return GetDirectionToNode(NextNode); }
    }

    //-------------//
    // Other Nodes //
    //-------------//
    /// <summary>
    /// Get the node in the parent path relative to the current location. For example, relative location 0 will return this node,
    /// -1 will return the previous node, and 1 will return the next node. Returns null in the event of a failure.
    /// </summary>
    /// <param name="relativeLocation">The relative location of the node to get.</param>
    /// <returns>The node at the given location.</returns>
    public PathNode GetNodeInParentPath(int relativeLocation)
    {
        // Handle a null parent path
        if (ParentPath == null || ParentPath.ChildNodes == null)
        {
            return null;
        }

        // Try to get the specified node
        try
        {
            int indexOfCurrentNode = ParentPath.ChildNodes.IndexOf(this);

            // If the current node is not in the parent list, return null
            if (indexOfCurrentNode == -1)
            {
                return null;
            }

            // Otherwise, attempt to get the specified node
            else
            {
                return ParentPath.ChildNodes[indexOfCurrentNode + relativeLocation];
            }
        }

        // If we can't get it, return null
        catch (IndexOutOfRangeException)
        {
            return null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
    public PathNode PreviousNode
    {
        get { return GetNodeInParentPath(-1); }
    }
    public PathNode NextNode
    {
        get { return GetNodeInParentPath(1); }
    }

    //----------//
    // Location //
    //----------//
    /// <summary>
    /// Updates the parent world to reflect the change in node location.
    /// </summary>
    /// <param name="oldRow">The old row of this node.</param>
    /// <param name="oldCol">The old column of this node.</param>
    /// <param name="newRow">The new row of this node.</param>
    /// <param name="newCol">The new column of this node.</param>
    private void UpdateReferenceInParentWorld(int oldRow, int oldCol, int newRow, int newCol)
    {
        World parentWorld = ParentPath.ParentWorld;

        // Remove the old entry
        parentWorld.RemovePathNodeFromWorld(this, oldRow, oldCol);

        // Add the new entry
        parentWorld.AddPathNodeToWorld(this, newRow, newCol);
    }

    private int _worldRow;
    public int WorldRow
    {
        get { return _worldRow; }
        set
        {
            UpdateReferenceInParentWorld(_worldRow, _worldCol, value, _worldCol);
            _worldRow = value;
        }
    }
    private int _worldCol;
    public int WorldCol
    {
        get { return _worldCol; }
        set
        {
            UpdateReferenceInParentWorld(_worldRow, _worldCol, _worldRow, value);
            _worldCol = value;
        }
    }

    //-------------//
    // Parent Path //
    //-------------//
    public Path ParentPath { get; private set; }
}

public class Path
{
    /// <summary>
    /// Instantiates a new Path object.
    /// </summary>
    /// <param name="parentWorld">The world that this path lives in.</param>
    public Path(World parentWorld)
    {
        ChildNodes = new List<PathNode>();
        ParentWorld = parentWorld;
    }

    //-------------//
    // Child Nodes //
    //-------------//
    /// <summary>
    /// Adds the given node to the end of the path.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AppendNewNode(PathNode node)
    {
        // Add the node if it's not already in the path
        if (!ChildNodes.Contains(node))
        {
            ChildNodes.Add(node);
        }
    }

    /// <summary>
    /// Removes the last node in the path.
    /// </summary>
    public void RemoveLastNode()
    {
        // If we have a last node, remove it
        PathNode lastNode = LastNode;
        if (lastNode != null)
        {
            ChildNodes.Remove(lastNode);
            ParentWorld.RemovePathNodeFromWorld(lastNode);
        }
    }

    /// <summary>
    /// Removes all child nodes from the path and from the parent world.
    /// </summary>
    public void RemoveAllNodes()
    {
        // Remove all the nodes from the world
        foreach (var node in ChildNodes)
        {
            ParentWorld.RemovePathNodeFromWorld(node);
        }

        // Remove all the nodes from the path
        ChildNodes.Clear();
    }

    /// <summary>
    /// Removes each node from the path that has one of the given roles.
    /// </summary>
    /// <param name="roles">The roles whose nodes should be removed.</param>
    public void RemoveAllNodesWithRole(params PathNode.Role[] roles)
    {
        // Create a list to hold all the remaining nodes
        List<PathNode> keptChildNodes = new List<PathNode>();

        // Check each node
        foreach (var node in ChildNodes)
        {
            // If this node's role is in our list, remove it from the world
            if (roles.Contains(node.NodeRole))
            {
                ParentWorld.RemovePathNodeFromWorld(node);
            }

            // Otherwise, keep the node
            else
            {
                keptChildNodes.Add(node);
            }
        }

        // Set our child nodes to the list containing all kept nodes
        ChildNodes = keptChildNodes;
    }

    public List<PathNode> ChildNodes { get; private set; }
    public PathNode FirstNode
    {
        get { return ChildNodes.Count > 0 ? ChildNodes[0] : null; }
    }
    public PathNode LastNode
    {
        get { return ChildNodes.Count > 0 ? ChildNodes[ChildNodes.Count - 1] : null; }
    }

    //--------------//
    // Parent World //
    //--------------//
    public World ParentWorld { get; private set; }
}

public class World
{
    /// <summary>
    /// Instantiates a new World instance.
    /// </summary>
    /// <param name="numRows">The number of rows in the world.</param>
    /// <param name="numCols">The number of columns in the world.</param>
    public World(int numRows, int numCols)
    {
        ChildPaths = new List<Path>();
        ChildNodes = new Dictionary<Tuple<int, int>, List<PathNode>>();

        NumRows = numRows;
        NumCols = numCols;
    }

    //-------------//
    // Child Nodes //
    //-------------//
    /// <summary>
    /// Adds the given node to the world. Will not add duplicates.
    /// </summary>
    /// <param name="node">The node to add.</param>
    public void AddPathNodeToWorld(PathNode node)
    {
        AddPathNodeToWorld(node, node.WorldRow, node.WorldCol);
    }

    /// <summary>
    /// Adds the given node to the world at a specific row and column. Will not add duplicates.
    /// </summary>
    /// <param name="node">The node to add.</param>
    /// <param name="row">The specific row to place the node at.</param>
    /// <param name="col">The specific column to place the node at.</param>
    public void AddPathNodeToWorld(PathNode node, int row, int col)
    {
        // Define our dictionary key
        Tuple<int, int> rowAndCol = new Tuple<int, int>(row, col);

        // If our dictionary already has an entry for this row and col and does not already contain
        // this node, add the node
        if (ChildNodes.ContainsKey(rowAndCol) && !ChildNodes[rowAndCol].Contains(node))
        {
            ChildNodes[rowAndCol].Add(node);
        }

        // Otherwise, create a new entry and add the node
        else
        {
            ChildNodes[rowAndCol] = new List<PathNode>();
            ChildNodes[rowAndCol].Add(node);
        }
    }

    /// <summary>
    /// Removes the given node from the world if possible.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    public void RemovePathNodeFromWorld(PathNode node)
    {
        RemovePathNodeFromWorld(node, node.WorldRow, node.WorldCol);
    }

    /// <summary>
    /// Removes the given node from the world at a specific row and column if possible.
    /// </summary>
    /// <param name="node">The node to remove.</param>
    /// <param name="row">The specific row to remove the node from.</param>
    /// <param name="col">The specific column to remove the node from.</param>
    public void RemovePathNodeFromWorld(PathNode node, int row, int col)
    {
        // Define our dictionary key
        Tuple<int, int> rowAndCol = new Tuple<int, int>(row, col);

        // If our dictionary has a entry for this row and col and contains the node, remove the node
        if (ChildNodes.ContainsKey(rowAndCol) && ChildNodes[rowAndCol].Contains(node))
        {
            ChildNodes[rowAndCol].Remove(node);
        }
    }

    /// <summary>
    /// Returns the first node at the given location in the world, or null if there are no nodes there.
    /// </summary>
    /// <param name="row">The row of the node to return.</param>
    /// <param name="col">The column of the node to return.</param>
    /// <returns>The first node at the given location.</returns>
    public PathNode GetNodeAtLocation(int row, int col)
    {
        // Get all nodes at this location
        List<PathNode> nodesAtLocation = GetNodesAtLocation(row, col);
        
        // If there are no nodes, return null
        if (nodesAtLocation.Count == 0)
        {
            return null;
        }

        // Otherwise, return the first node
        else
        {
            return nodesAtLocation[0];
        }
    }

    /// <summary>
    /// Returns a list of all nodes at the given location in the world.
    /// </summary>
    /// <param name="row">The row of the nodes to return.</param>
    /// <param name="col">The column of the nodes to return.</param>
    /// <returns>A list of all nodes at the given location.</returns>
    public List<PathNode> GetNodesAtLocation(int row, int col)
    {
        // Define our dictionary key
        Tuple<int, int> rowAndCol = new Tuple<int, int>(row, col);

        // If our dictionary has a non-null entry for this row and col, return the entry
        if (ChildNodes.ContainsKey(rowAndCol) && ChildNodes[rowAndCol] != null)
        {
            return ChildNodes[rowAndCol];
        }

        // Otherwise, return an empty list
        else
        {
            return new List<PathNode>();
        }
    }

    private Dictionary<Tuple<int, int>, List<PathNode>> ChildNodes;

    //-------------//
    // Child Paths //
    //-------------//
    /// <summary>
    /// Removes all child paths and their nodes from the world.
    /// </summary>
    public void RemoveAllPaths()
    {
        // Remove all child nodes of the child paths
        foreach (var path in ChildPaths)
        {
            path.RemoveAllNodes();
        }

        // Remove all the paths from the world
        ChildPaths.Clear();
    }

    public List<Path> ChildPaths { get; private set; }

    //------------//
    // Dimensions //
    //------------//
    public int NumRows { get; set; }
    public int NumCols { get; set; }
}

public class PathFindingOptions
{
    /// <summary>
    /// Instantiates a new PathFindingOptions object. Both numRows and numCols must be given values,
    /// and at least endingRow or endingCol must also be given a value.
    /// </summary>
    /// <param name="numRows">The number of rows in the world.</param>
    /// <param name="numCols">The number of columns in the world.</param>
    /// <param name="startingRow">The starting row for the paths.</param>
    /// <param name="startingCol">The starting column for the paths.</param>
    /// <param name="endingRow">The ending row for the paths.</param>
    /// <param name="endingCol">The ending column for the paths.</param>
    /// <param name="maxNumberOfPaths">The maximum number of paths to generate.</param>
    /// <param name="allowIntersection">Whether or not to allow paths to intersect other paths. Note that all paths will intersect at the starting location.
    /// Path can never intersect themselves.</param>
    /// <exception cref="ArgumentException">Thrown if some of the provided arguments are invalid.</exception>
    public PathFindingOptions(int numRows, int numCols, int startingRow, int startingCol, Nullable<int> endingRow = null, Nullable<int> endingCol = null, int maxNumberOfPaths = 1, bool allowIntersection = false)
    {
        // Ensure we have a valid ending row or col
        if (endingRow == null && endingCol == null)
        {
            throw new ArgumentException("The arguments endingRow and endingCol cannot both be null.");
        }

        // Ensure we have valid numbers of rows and cols
        if (numRows <= 0)
        {
            throw new ArgumentException(string.Format("Invalid number of rows '{0}'. Number of rows must be a positive integer.", numRows));
        }
        if (numCols <= 0)
        {
            throw new ArgumentException(string.Format("Invalid number of columns '{0}'. Number of columns must be a positive integer.", numCols));
        }

        // Ensure we have a valid max number of paths
        if (maxNumberOfPaths <= 0)
        {
            throw new ArgumentException(string.Format("Invalid maximum number of paths '{0}'. Maximum number of paths must be a positive integer.", maxNumberOfPaths));
        }

        // Ensure our starting row and col are constrained to inside the world
        if (startingRow < 0 || startingRow >= numRows)
        {
            throw new ArgumentException(string.Format("Invalid start row '{0}'. Start row must be between zero (inclusive) and the number of rows '{1}' (exclusive).", startingRow, numRows));
        }
        if (startingCol < 0 || startingCol >= numCols)
        {
            throw new ArgumentException(string.Format("Invalid start column '{0}'. Start column must be between zero (inclusive) and the number of columns '{1}' (exclusive).", startingCol, numCols));
        }

        // Ensure our ending row and col are constrained to inside the world
        if (endingRow != null && (endingRow < 0 || endingRow >= numRows))
        {
            throw new ArgumentException(string.Format("Invalid ending row '{0}'. Ending row must be between zero (inclusive) and the number of rows '{1}' (exclusive).", endingRow, numRows));
        }
        if (endingCol != null && (endingCol < 0 || endingCol >= numCols))
        {
            throw new ArgumentException(string.Format("Invalid start column '{0}'. Start column must be between zero (inclusive) and the number of columns '{1}' (exclusive).", endingCol, numCols));
        }

        // Assign data properties
        NumRows = numRows;
        NumCols = numCols;
        StartingRow = startingRow;
        StartingCol = startingCol;
        EndingRow = endingRow;
        EndingCol = endingCol;
        MaxNumberOfPaths = maxNumberOfPaths;
        AllowIntersection = allowIntersection;
    }

    //------//
    // Data //
    //------//
    public int NumRows { get; set; }
    public int NumCols { get; set; }

    public int StartingRow { get; set; }
    public int StartingCol { get; set; }

    public Nullable<int> EndingRow { get; set; }
    public Nullable<int> EndingCol { get; set; }

    public int MaxNumberOfPaths { get; set; }
    public bool AllowIntersection { get; set; }
}
