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
        NORMAL,
        START,
        END,
        TEMPORARY
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

    //-------//
    // Nodes //
    //-------//
    /// <summary>
    /// Get the node in the parent path relative to the current location. For example, relative location 0 will return this node,
    /// -1 will return the previous node, and 1 will return the next node. Returns null in the event of a failure.
    /// </summary>
    /// <param name="relativeLocation">The relative location of the node to get.</param>
    /// <returns>The node at the given location.</returns>
    private PathNode GetNodeInParentPath(int relativeLocation)
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
    }
    public PathNode PreviousNode
    {
        get { return GetNodeInParentPath(-1); }
    }
    public PathNode NextNode
    {
        get { return GetNodeInParentPath(1); }
    }

    //------//
    // Data //
    //------//
    public Path ParentPath { get; set; }
    public int WorldRow { get; set; }
    public int WorldCol { get; set; }
}

public class Path
{
    /// <summary>
    /// Instantiates a new Path object.
    /// </summary>
    /// <param name="childNodes">The path nodes that make up the path.</param>
    /// <param name="parentWorld">The world that this path lives in.</param>
    public Path(List<PathNode> childNodes, World parentWorld)
    {
        ChildNodes = childNodes;
        ParentWorld = parentWorld;
    }

    //------//
    // Data //
    //------//
    public List<PathNode> ChildNodes { get; set; }
    public World ParentWorld { get; set; }
}

public class World
{
    /// <summary>
    /// Instantiates a new World instance.
    /// </summary>
    /// <param name="childPaths">The paths that make up the world.</param>
    public World(List<Path> childPaths)
    {
        ChildPaths = childPaths;
    }

    //-------//
    // Nodes //
    //-------//
    /// <summary>
    /// Gets the first node found at the given location.
    /// </summary>
    /// <param name="row">The row of the node to get.</param>
    /// <param name="col">The col of the node to get.</param>
    /// <returns>The first node found at the given location.</returns>
    public PathNode GetNodeAtLocation(int row, int col)
    {
        return GetNodesAtLocation(row, col).FirstOrDefault();
    }

    /// <summary>
    /// Gets all nodes found at the given location.
    /// </summary>
    /// <param name="row">The row of the nodes to get.</param>
    /// <param name="col">The col of the nodes to get.</param>
    /// <returns>All nodes found at the given location.</returns>
    public IEnumerable<PathNode> GetNodesAtLocation(int row, int col)
    {
        return from path in ChildPaths
               from node in path.ChildNodes
               where node.WorldRow == row && node.WorldCol == col
               select node;
    }

    //------//
    // Data //
    //------//
    List<Path> ChildPaths { get; set; }
}

public class PathFindingOptions
{
    /// <summary>
    /// Instantiates a new PathFindingOptions object. Both numRows and numCols must be given values,
    /// and at least endingRow or endingCol must also be given a value.
    /// </summary>
    /// <param name="numRows">The number of rows in the world.</param>
    /// <param name="numCols">The number of columns in the world.</param>
    /// <param name="endingRow">The ending row for the paths.</param>
    /// <param name="endingCol">The ending column for the paths.</param>
    /// <param name="maxNumberOfPaths">The maximum number of paths to generate.</param>
    /// <param name="allowIntersection">Whether or not to allow paths to intersect.</param>
    /// <exception cref="ArgumentException">Thrown if endingRow and endingCol are both null.</exception>
    public PathFindingOptions(int numRows, int numCols, Nullable<int> endingRow = null, Nullable<int> endingCol = null, int maxNumberOfPaths = 1, bool allowIntersection = false)
    {
        // Ensure we have a valid ending row / col
        if (endingRow == null && endingCol == null)
        {
            throw new ArgumentException("The arguments endingRow and endingCol cannot both be null.");
        }

        // Assign data properties
        NumRows = numRows;
        NumCols = numCols;
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

    public Nullable<int> EndingRow { get; set; }
    public Nullable<int> EndingCol { get; set; }

    public int MaxNumberOfPaths { get; set; }
    public bool AllowIntersection { get; set; }
}
