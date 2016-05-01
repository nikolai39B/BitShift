//using UnityEngine;
//using System;
//using System.Text;
//using System.Collections.Generic;

//public class GameGenerator : MonoBehaviour
//{
//    //---------------//
//    // World Options //
//    //---------------//
//    public int rowsInWorld = 26;
//    public int columnsInWorld = 26;

//    public int startRow = 0;
//    public int startCol = 13;

//    public int endRow = 25;
//    public bool useEndRow = true;
//    public int endCol = 0;
//    public bool useEndCol = false;

//    public int maxNumberOfPaths = 3;
//    public bool allowPathIntersection = false;

//    void Awake()
//    {
//        // Bulid the options
//        PathFindingOptions options = new PathFindingOptions(
//            rowsInWorld, columnsInWorld, startRow, startCol, 
//            useEndRow ? (int?)endRow : null, 
//            useEndCol ? (int?)endCol : null, 
//            maxNumberOfPaths, allowPathIntersection);

//        // Build the world
//        World world = GenerateWorld(options);

//        GetComponent<GameGeneratorDebug>().CreateTilesetFromWorld(world);

//        /* Print the world
//        for (int row = 0; row < world.NumRows; row++)
//        {
//            StringBuilder strBuilder = new StringBuilder();
//            for (int col = 0; col < world.NumCols; col++)
//            {
//                // Get the node (if any) at this location
//                PathNode currentNode = world.GetNodeAtLocation(row, col);

//                // If there's no node, print that
//                if (currentNode == null)
//                {
//                    strBuilder.Append(" ---");
//                }

//                // If the node is the start node, print that
//                else if (currentNode.NodeRole == PathNode.Role.START)
//                {
//                    strBuilder.Append(" SS");
//                }

//                // Otherwise, determine what path it belongs to, and map it to a character
//                else
//                {
//                    int pathIndex = world.ChildPaths.IndexOf(currentNode.ParentPath);
//                    char dirChar;
//                    switch (currentNode.DirToNextNode)
//                    {
//                        case Direction.UP:
//                            dirChar = '^';
//                            break;
//                        case Direction.RIGHT:
//                            dirChar = '>';
//                            break;
//                        case Direction.DOWN:
//                            dirChar = 'v';
//                            break;
//                        case Direction.LEFT:
//                            dirChar = '<';
//                            break;
//                        default:
//                            dirChar = 'X';
//                            break;
//                    }
//                    strBuilder.Append(string.Format(" {0}{1}", pathIndex, dirChar));
//                }
//            }
//            Debug.Log(strBuilder.ToString());
//        }*/


//    }

//    /// <summary>
//    /// Builds paths through an empty world, and returns the world.
//    /// </summary>
//    /// <param name="options">The options for generating the paths.</param>
//    /// <returns>The world with the newly generated paths.</returns>
//    public World GenerateWorld(PathFindingOptions options)
//    {
//        // Create the new world
//        World world = new World(options.NumRows, options.NumCols);

//        // Start generating paths
//        bool pathGenerationSucceeded = true;
//        Path blockingNodes = new Path(world);

//        // Generate paths until we hit the maximum or path generation fails
//        while (world.ChildPaths.Count < options.MaxNumberOfPaths && pathGenerationSucceeded)
//        {
//            // Attempt to generate the next path
//            Path generatedPath;
//            pathGenerationSucceeded = GeneratePath(out generatedPath, world, blockingNodes, options);
//        }

//        // Destroy all blocking nodes
//        blockingNodes.RemoveAllNodes();

//        return world;
//    }

//    /// <summary>
//    /// Attempts to generate a new path through the world.
//    /// </summary>
//    /// <param name="generatedPath">The newly generated path.</param>
//    /// <param name="world">The world to build the path in.</param>
//    /// <param name="blockingNodes">An auxilary path used to hold the blocking nodes.</param>
//    /// <param name="options">The options for generating the path.</param>
//    /// <returns>True if path generation succeeded, false otherwise.</returns>
//    private bool GeneratePath(out Path generatedPath, World world, Path blockingNodes, PathFindingOptions options)
//    {
//        // Create the new path and add it to the world
//        generatedPath = new Path(world);
//        world.ChildPaths.Add(generatedPath);

//        // Add the first "start" node to the path
//        PathNode startNode = new PathNode(generatedPath, options.StartingRow, options.StartingCol, PathNode.Role.START);
//        generatedPath.AppendNewNode(startNode);

//        // Declare iteration variables
//        bool addNewNode = true;
//        bool pathGeneratedSuccessfully = false;

//        // Loop and add nodes until we are told to stop
//        while (addNewNode)
//        {

//            PathNode newNode;
//            PathGenerationStatus newStatus = AddNodeToPath(out newNode, world, generatedPath, blockingNodes, options);

//            // If we added or removed a node, try to add another one
//            if (newStatus == PathGenerationStatus.ADDED_NODE || newStatus == PathGenerationStatus.REMOVED_NODE)
//            {
//                // Add another node
//                addNewNode = true;
//            }

//            // If the path is finished or failed, stop adding nodes
//            else
//            {
//                // Stop adding nodes
//                addNewNode = false;

//                // Log if we succeeded at generating the path
//                pathGeneratedSuccessfully = newStatus == PathGenerationStatus.PATH_FINISHED;
//            }
//        }

//        // If we generated the path, return success
//        if (pathGeneratedSuccessfully)
//        {
//            // First, remove all temporary nodes
//            generatedPath.RemoveAllNodesWithRole(PathNode.Role.TEMPORARY);

//            return true;
//        }

//        // Otherwise, we failed to generate the path, so remove all references to it and return failure
//        else
//        {
//            generatedPath.RemoveAllNodes();
//            world.ChildPaths.Remove(generatedPath);
//            generatedPath = null;

//            return false;
//        }
//    }

//    /// <summary>
//    /// Attempts to add a new node to the current path.
//    /// </summary>
//    /// <param name="newNode">The newly generated node.</param>
//    /// <param name="world">The wold to build the node in.</param>
//    /// <param name="path">The path to add the new node to. The path should contain at least a "START" node.</param>
//    /// <param name="blockingNodes">An auxilary path used to hold the blocking nodes.</param>
//    /// <param name="options">The options for generating the node.</param>
//    /// <returns>The result of trying to add a new node.</returns>
//    private PathGenerationStatus AddNodeToPath(out PathNode newNode, World world, Path path, Path blockingNodes, PathFindingOptions options)
//    {
//        // Get the row and col for the last node
//        PathNode lastNode = path.LastNode;
//        int prevRow = path.LastNode.WorldRow;
//        int prevCol = path.LastNode.WorldCol;

//        // Determine which directions are valid to move in
//        List<Direction> validDirections = new List<Direction>();
//        foreach (var dirToCheck in new Direction[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT })
//        {
//            // Get the new coordinates for this direction
//            int rowInDirection = prevRow;
//            int colInDirection = prevCol;
//            GetNewCoordinatesFromDirection(ref rowInDirection, ref colInDirection, dirToCheck);

//            // See if we can add a node here
//            if (CanPlaceNodeAtLocation(world, path, rowInDirection, colInDirection, options))
//            {
//                validDirections.Add(dirToCheck);
//            }
//        }

//        // Make sure we can move in at least one direction
//        if (validDirections.Count == 0)
//        {
//            // If we can't, and we're at the start, we've failed to create the path
//            if (path.LastNode.NodeRole == PathNode.Role.START)
//            {
//                newNode = null;
//                return PathGenerationStatus.PATH_FAILED;
//            }

//            // Otherwise, remove this node, block the direction to it for the previous node, 
//            // and return that we've removed a node
//            else
//            {
//                newNode = null;

//                // Change the last node of the current path into a block node in our auxilary path
//                PathNode newBlockingNode = new PathNode(blockingNodes, lastNode.WorldRow, lastNode.WorldCol, PathNode.Role.BLOCK);
//                blockingNodes.AppendNewNode(newBlockingNode);

//                // Remove the last node
//                path.RemoveLastNode();

//                return PathGenerationStatus.REMOVED_NODE;
//            }
//        }

//        // If we can, add a new node
//        else
//        {
//            // Get the direction to move
//            int directionIndex = UnityEngine.Random.Range(0, validDirections.Count);
//            Direction directionToMove = validDirections[directionIndex];

//            // Get the coordinates for the next node
//            int nextRow = prevRow;
//            int nextCol = prevCol;
//            GetNewCoordinatesFromDirection(ref nextRow, ref nextCol, directionToMove);

//            // Create the next node and add it to the path
//            newNode = new PathNode(path, nextRow, nextCol);
//            path.AppendNewNode(newNode);

//            // If we hit the end, update the node role and return path complete
//            if ((options.EndingRow == null || options.EndingRow == newNode.WorldRow) &&
//                (options.EndingCol == null || options.EndingCol == newNode.WorldCol))
//            {
//                newNode.NodeRole = PathNode.Role.END;
//                return PathGenerationStatus.PATH_FINISHED;
//            }

//            // Otherwise, return that we added a node
//            else
//            {
//                return PathGenerationStatus.ADDED_NODE;
//            }
//        }
//    }

//    /// <summary>
//    /// Checks whether or not we can place a node at the given location.
//    /// </summary>
//    /// <param name="world">The current world to place a node in.</param>
//    /// <param name="currentPath">The path that the node would belong to.</param>
//    /// <param name="row">The row to place the node in.</param>
//    /// <param name="col">The column to place the node in.</param>
//    /// <param name="options">The path finding options.</param>
//    /// <returns>True if we can place a node there, false otherwise.</returns>
//    private bool CanPlaceNodeAtLocation(World world, Path path, int row, int col, PathFindingOptions options)
//    {
//        // Check for moving off the world
//        if (row < 0 || row >= options.NumRows ||
//            col < 0 || col >= options.NumCols)
//        {
//            return false;
//        }

//        // Get the nodes at the new location
//        List<PathNode> nodesAtLocation = world.GetNodesAtLocation(row, col);

//        // Check these nodes for intersection conflicts
//        foreach (var node in nodesAtLocation)
//        {
//            // If this node is a block node, return false
//            if (node.NodeRole == PathNode.Role.BLOCK)
//            {
//                return false;
//            }
            
//            // If we would intersect with ourself, return false
//            if (node.ParentPath == path)
//            {
//                return false;
//            }

//            // If we would intersect with a different path and we're not allowed to do so, return false
//            if (node.ParentPath != path && !options.AllowIntersection)
//            {
//                return false;
//            }
//        }

//        // If we got here, than we can move here
//        return true;
//    }

//    /// <summary>
//    /// Gets new coordinates after a movement of a single unit in the given direction.
//    /// </summary>
//    /// <param name="currentRow">The current row coordinate.</param>
//    /// <param name="currentColumn">The current column coordinate.</param>
//    /// <param name="directionToMove">The direction to move.</param>
//    /// <exception cref="ArgumentException">Thrown when the direction to move is not recognized by the function.</exception>
//    private void GetNewCoordinatesFromDirection(ref int currentRow, ref int currentColumn, Direction directionToMove)
//    {
//        switch (directionToMove)
//        {
//            case Direction.UP:
//                currentRow--;
//                break;

//            case Direction.DOWN:
//                currentRow++;
//                break;

//            case Direction.LEFT:
//                currentColumn--;
//                break;

//            case Direction.RIGHT:
//                currentColumn++;
//                break;

//            case Direction.NONE:
//                break;

//            default:
//                // We should never be here
//                throw new ArgumentException(string.Format("Direction to check '{0}' was not recognized.", directionToMove));
//        }
//    }

//    /// <summary>
//    /// Gets the reverse of the given direction (UP to DOWN, etc.).
//    /// </summary>
//    /// <param name="directionToReverse">The direction to reverse.</param>
//    /// <returns>The reverse of the given direction.</returns>
//    private Direction ReverseDirection(Direction directionToReverse)
//    {
//        switch (directionToReverse)
//        {
//            case Direction.UP:
//                return Direction.DOWN;

//            case Direction.RIGHT:
//                return Direction.LEFT;

//            case Direction.DOWN:
//                return Direction.UP;

//            case Direction.LEFT:
//                return Direction.RIGHT;

//            case Direction.NONE:
//                return Direction.NONE;

//            default:
//                throw new ArgumentException(string.Format("Direction to reverse '{0}' was not recognized.", directionToReverse));
//        }
//    }
//}
