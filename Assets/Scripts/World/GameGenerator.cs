using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;

public class GameGenerator : MonoBehaviour
{
	void Start()
    {
        /*int rows = 5;
        int cols = 5;
        int maxNoOfPaths = 3;
        int startRow = 4;
        int startCol = 2;
        Direction endingSide = Direction.UP;
        int numberOfPathsGenerated;

        PathNode[,] world = GeneratePaths(rows, cols, maxNoOfPaths, startRow, startCol, endingSide, out numberOfPathsGenerated);

        for (int row = 0; row < world.GetLength(0); row++)
        {
            StringBuilder strBuilder = new StringBuilder();
            for (int col = 0; col < world.GetLength(1); col++)
            {
                PathNode currentNode = world[row, col];
                if (currentNode == null)
                {
                    strBuilder.Append(" -");
                }
                else if (currentNode.NodeRole == PathNode.Role.START)
                {
                    strBuilder.Append(" S");
                }
                else
                {
                    strBuilder.Append(string.Format(" {0}", currentNode.PathId));
                }
            }
            Debug.Log(strBuilder.ToString());
        }*/
    }

    /// <summary>
    /// Builds paths through an empty world, and returns the world.
    /// </summary>
    /// <param name="options">The options for generating the paths.</param>
    /// <returns>The world with the newly generated paths.</returns>
    public World GeneratePaths(PathFindingOptions options)
    {
        // Create the new world
        World world = new World(new List<Path>());

        // Start generating paths
        bool pathGenerationSucceeded = true;

        // Generate paths until we hit the maximum or path generation fails
        while (world.ChildPaths.Count < options.MaxNumberOfPaths && pathGenerationSucceeded)
        {
            // Attempt to generate the next path
            Path generatedPath;
            pathGenerationSucceeded = GeneratePath(out generatedPath, world, options);
        }

        return world;
    }

    /// <summary>
    /// Attempts to generate a new path through the world.
    /// </summary>
    /// <param name="generatedPath">The newly generated path.</param>
    /// <param name="world">The world to build the path in.</param>
    /// <param name="options">The options for generating the path.</param>
    /// <returns>True if path generation succeeded, false otherwise.</returns>
    private bool GeneratePath(out Path generatedPath, World world, PathFindingOptions options)
    {
        /* Diagram of World Coordinates and Directions
         *
         * array[5,4]
         *
         *         UP       
         * L [ 00 01 02 03 ] R
         * E [ 10 11 12 13 ] I
         * F [ 20 21 22 23 ] G
         * T [ 30 31 32 33 ] H
         *   [ 40 41 42 43 ] T
         *        DOWN
         *
         * World Dimension 0 Length (# of Rows): 5
         * World Dimension 1 Length (# of Cols): 4
        */

        // Create the new path and add it to the world
        generatedPath = new Path(new List<PathNode>(), world);
        world.ChildPaths.Add(generatedPath);

        // Add the first "start" node to the path
        PathNode startNode = new PathNode(generatedPath, options.StartingRow, options.StartingCol, PathNode.Role.START);
        generatedPath.AppendNewNode(startNode);

        // Declare iteration variables
        bool addNewNode = true;
        bool pathGeneratedSuccessfully = false;

        // Loop and add nodes until we are told to stop
        while (addNewNode)
        {

            PathNode newNode;
            PathGenerationStatus newStatus = AddNodeToPath(out newNode, world, generatedPath, options);

            // If we added or removed a node, try to add another one
            if (newStatus == PathGenerationStatus.ADDED_NODE || newStatus == PathGenerationStatus.REMOVED_NODE)
            {
                // Add another node
                addNewNode = true;
            }

            // If the path is finished or failed, stop adding nodes
            else
            {
                // Stop adding nodes
                addNewNode = false;

                // Log if we succeeded at generating the path
                pathGeneratedSuccessfully = newStatus == PathGenerationStatus.PATH_FINISHED;
            }
        }

        // If we generated the path, return success
        if (pathGeneratedSuccessfully)
        {
            // First, remove all temporary and block nodes
            generatedPath.RemoveAllNodesWithRole(PathNode.Role.TEMPORARY, PathNode.Role.BLOCK);

            return true;
        }

        // Otherwise, we failed to generate the path, so remove all references to it and return failure
        else
        {
            generatedPath.RemoveAllNodes();
            world.ChildPaths.Remove(generatedPath);
            generatedPath = null;

            return false;
        }
    }

    /// <summary>
    /// Attempts to add a new node to the current path.
    /// </summary>
    /// <param name="newNode">The newly generated node.</param>
    /// <param name="world">The wold to build the node in.</param>
    /// <param name="path">The path to add the new node to. The path should contain at least a "START" node.</param>
    /// <param name="options">The options for generating the node.</param>
    /// <returns>The result of trying to add a new node.</returns>
    private PathGenerationStatus AddNodeToPath(out PathNode newNode, World world, Path path, PathFindingOptions options)
    {
        newNode = new PathNode(path, 0, 0);

        // Find the last node that is not temporary or block
        PathNode lastValidNode = path.LastNode;
        while (lastValidNode.NodeRole == PathNode.Role.TEMPORARY || lastValidNode.NodeRole == PathNode.Role.BLOCK)
        {
            lastValidNode = lastValidNode.PreviousNode;
        }

        // Get the row and col for this node
        int prevRow = lastValidNode.WorldRow;
        int prevCol = lastValidNode.WorldCol;

        // Determine which directions are valid to move in
        List<Direction> validDirections = new List<Direction>();
        foreach (var dirToCheck in new Direction[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT })
        {
            // Get the new coordinates for this direction
            int rowInDirection = prevRow;
            int colInDirection = prevCol;
            GetNewCoordinatesFromDirection(ref rowInDirection, ref colInDirection, dirToCheck);

            // See if we can add a node here
            if (CanPlaceNodeAtLocation(world, path, rowInDirection, colInDirection, options))
            {
                validDirections.Add(dirToCheck);
            }
        }

        // Make sure we can move in at least one direction
        if (validDirections.Count == 0)
        {
            // If we can't, and we're at the start, we've failed to create the path
            if (lastValidNode.NodeRole == PathNode.Role.START)
            {
                return PathGenerationStatus.PATH_FAILED;
            }

            // Otherwise, turn this node into a block node and return that we've removed a node
            else
            {
                lastValidNode.NodeRole = PathNode.Role.BLOCK;
                return PathGenerationStatus.REMOVED_NODE;
            }
        }

        // If we can, add a new node
        else
        {
            // TODO: finish implementation
        }

        return PathGenerationStatus.ADDED_NODE;
    }

    /// <summary>
    /// Builds a given number of paths through a new world with the specified dimensions.
    /// </summary>
    /// <param name="rows">The number of rows in the world.</param>
    /// <param name="columns">The number of columns in the world.</param>
    /// <param name="maxNumberOfPaths">The maximum number of paths to place in the world. If the no more paths can be generated, the method will terminate early.</param>
    /// <param name="startRow">The starting row for the paths.</param>
    /// <param name="startColumn">The starting column for the paths.</param>
    /// <param name="endingSide">The ending side for the paths.</param>
    /// <param name="numberOfPathsGenerated">The number of paths actually generated by the method.</param>
    /// <exception cref="ArgumentException">Thrown when one or more given arguments is invalid.</exception>
    /// <returns>The new world, with the specified number of paths running through it. The start location is marked as a PathNode with directions 'NONE' and id '-1'.</returns>
    public PathNode[,] GeneratePaths(int rows, int columns, int maxNumberOfPaths, int startRow, int startColumn, Direction endingSide, out int numberOfPathsGenerated)
    {
        // Verify argument validity
        if (rows <= 0)
        {
            throw new ArgumentException(string.Format("Invalid number of rows '{0}'. Number of rows must be a positive integer.", rows));
        }
        if (columns <= 0)
        {
            throw new ArgumentException(string.Format("Invalid number of columns '{0}'. Number of columns must be a positive integer.", columns));
        }
        if (maxNumberOfPaths <= 0)
        {
            throw new ArgumentException(string.Format("Invalid maximum number of paths '{0}'. Maximum number of paths must be a positive integer.", maxNumberOfPaths));
        }
        if (startRow < 0 || startRow >= rows)
        {
            throw new ArgumentException(string.Format("Invalid start row '{0}'. Start row must be between zero (inclusive) and the number of rows '{1}' (exclusive).", startRow, rows));
        }
        if (startColumn < 0 || startColumn >= columns)
        {
            throw new ArgumentException(string.Format("Invalid start column '{0}'. Start column must be between zero (inclusive) and the number of columns '{1}' (exclusive).", startColumn, columns));
        }
        if (endingSide == Direction.NONE)
        {
            throw new ArgumentException("Ending side cannot be 'Direction.NONE'.");
        }

        // Instantiate the world
        PathNode[,] world = new PathNode[rows, columns];

        // Place the start location
        world[startRow, startColumn] = new PathNode(Direction.NONE, Direction.NONE, -1, PathNode.Role.START);

        // Add the paths
        numberOfPathsGenerated = 0;
        while (numberOfPathsGenerated < maxNumberOfPaths)
        {
            // Add this iteration's path
            bool pathGenerated = AddPathToWorld(ref world, startRow, startColumn, endingSide, numberOfPathsGenerated);

            if (pathGenerated)
            {
                // If we manage to add a path to the world, increment the numberOfPathsGenerated variable
                numberOfPathsGenerated++;
            }
            else
            {
                // Otherwise, break out early
                break;
            }
        }

        // Return our new world with the generated paths
        return world;
    }

    /// <summary>
    /// Generate and add a new path the the given world.
    /// </summary>
    /// <param name="world">The world to add a path to.</param>
    /// <param name="startRow">The path start row.</param>
    /// <param name="startColumn">The path start column.</param>
    /// <param name="endingSide">The side that the path ends on.</param>
    /// <param name="pathId">The id for the path to generate.</param>
    /// <returns>True if the path was generated and added successfully; false otherwise.</returns>
    private bool AddPathToWorld(ref PathNode[,] world, int startRow, int startColumn, Direction endingSide, int pathId)
    {
        /* Diagram of World Coordinates and Directions
         *
         * array[5,4]
         *
         *         UP       
         * L [ 00 01 02 03 ] R
         * E [ 10 11 12 13 ] I
         * F [ 20 21 22 23 ] G
         * T [ 30 31 32 33 ] H
         *   [ 40 41 42 43 ] T
         *        DOWN
         *
         * World Dimension 0 Length (# of Rows): 5
         * World Dimension 1 Length (# of Cols): 4
        */

        // Pull relevant info out of the world
        int rowsInWorld = world.GetLength(0);
        int colsInWorld = world.GetLength(1);

        // Define values for iterations
        bool addNewNode = true;
        bool pathCouldNotBeGenerated = false;
        int currentRow = startRow;
        int currentColumn = startColumn;
        Direction directionToLastNode = Direction.NONE;

        while (addNewNode)
        {
            // Determine which directions are valid
            List<Direction> validDirections = new List<Direction>();
            foreach (var dirToCheck in new Direction[] { Direction.UP, Direction.RIGHT, Direction.DOWN, Direction.LEFT })
            {
                if (CanMoveInDirection(world, currentRow, currentColumn, dirToCheck, endingSide))
                {
                    validDirections.Add(dirToCheck);
                }
            }

            if (validDirections.Count == 0)
            {
                // If we're at the start, then we can't add a new path
                if (currentRow == startRow && currentColumn == startColumn)
                {
                    addNewNode = false;
                    pathCouldNotBeGenerated = true;
                    break;
                }

                // Otherwise, step back
                else
                {
                    // Get the row and column for the previous node
                    int previousRow = currentRow;
                    int previousColumn = currentColumn;
                    GetNewCoordinatesFromDirection(ref previousRow, ref previousColumn, directionToLastNode);

                    // Set a temporary node at our current location so we don't go back here
                    PathNode temporaryNodeToAdd = new PathNode(Direction.NONE, Direction.NONE, pathId, PathNode.Role.TEMPORARY);
                    world[currentRow, currentColumn] = temporaryNodeToAdd;

                    // Set the current row and column for the next iteration
                    currentRow = previousRow;
                    currentColumn = previousColumn;
                    continue;
                }
            }

            // Get the direction to move
            int directionIndex = UnityEngine.Random.Range(0, validDirections.Count);
            Direction directionToMove = validDirections[directionIndex];

            // Add a new node at our current location (if we're not at the start)
            if (currentRow != startRow || currentColumn != startColumn)
            {
                PathNode nodeToAdd = new PathNode(directionToMove, directionToLastNode, pathId);
                world[currentRow, currentColumn] = nodeToAdd;
            }

            // Update direction to last node for next iteration (so the next node can point at us)
            directionToLastNode = ReverseDirection(directionToMove);

            // Get the new coordinates and create a node
            int targetRow = currentRow;
            int targetColumn = currentColumn;
            GetNewCoordinatesFromDirection(ref targetRow, ref targetColumn, directionToMove);

            // If we're off the world, then we must be done with this path
            if (targetRow < 0 ||
                targetRow >= rowsInWorld ||
                targetColumn < 0 ||
                targetColumn >= colsInWorld)
            {
                addNewNode = false;
                break;
            }

            // Set the current row and column for the next iteration
            currentRow = targetRow;
            currentColumn = targetColumn;
        }

        RemoveTemporaryPathNodes(ref world);

        return !pathCouldNotBeGenerated;
    }

    /// <summary>
    /// Checks whether or not we can place a node at the given location.
    /// </summary>
    /// <param name="world">The current world to place a node in.</param>
    /// <param name="currentPath">The path that the node would belong to.</param>
    /// <param name="row">The row to place the node in.</param>
    /// <param name="col">The column to place the node in.</param>
    /// <param name="options">The path finding options.</param>
    /// <returns>True if we can place a node there, false otherwise.</returns>
    private bool CanPlaceNodeAtLocation(World world, Path path, int row, int col, PathFindingOptions options)
    {
        // Check for moving off the world
        if (row < 0 || row >= options.NumRows ||
            col < 0 || col >= options.NumCols)
        {
            return false;
        }

        // Get the nodes at the new location
        List<PathNode> nodesAtLocation = world.GetNodesAtLocation(row, col);

        // Check these nodes for intersection conflicts
        foreach (var node in nodesAtLocation)
        {
            // If we would intersect with ourself, return false
            if (node.ParentPath == path)
            {
                return false;
            }

            // If this node is a block node, return false
            if (node.NodeRole == PathNode.Role.BLOCK)
            {
                return false;
            }

            // If we would intersect with a different path and we're not allowed to do so, return false
            if (node.ParentPath != path && !options.AllowIntersection)
            {
                return false;
            }
        }

        // If we got here, than we can move here
        return true;
    }

    /// <summary>
    /// Gets new coordinates after a movement of a single unit in the given direction.
    /// </summary>
    /// <param name="currentRow">The current row coordinate.</param>
    /// <param name="currentColumn">The current column coordinate.</param>
    /// <param name="directionToMove">The direction to move.</param>
    /// <exception cref="ArgumentException">Thrown when the direction to move is not recognized by the function.</exception>
    private void GetNewCoordinatesFromDirection(ref int currentRow, ref int currentColumn, Direction directionToMove)
    {
        switch (directionToMove)
        {
            case Direction.UP:
                currentRow--;
                break;

            case Direction.DOWN:
                currentRow++;
                break;

            case Direction.LEFT:
                currentColumn--;
                break;

            case Direction.RIGHT:
                currentColumn++;
                break;

            case Direction.NONE:
                break;

            default:
                // We should never be here
                throw new ArgumentException(string.Format("Direction to check '{0}' was not recognized.", directionToMove));
        }
    }

    /// <summary>
    /// Gets the reverse of the given direction (UP to DOWN, etc.).
    /// </summary>
    /// <param name="directionToReverse">The direction to reverse.</param>
    /// <returns>The reverse of the given direction.</returns>
    private Direction ReverseDirection(Direction directionToReverse)
    {
        switch (directionToReverse)
        {
            case Direction.UP:
                return Direction.DOWN;

            case Direction.RIGHT:
                return Direction.LEFT;

            case Direction.DOWN:
                return Direction.UP;

            case Direction.LEFT:
                return Direction.RIGHT;

            case Direction.NONE:
                return Direction.NONE;

            default:
                throw new ArgumentException(string.Format("Direction to reverse '{0}' was not recognized.", directionToReverse));
        }
    }
}
