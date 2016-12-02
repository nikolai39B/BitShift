using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour
{
    //------------------//
    // Build Parameters //
    //------------------//

    public int rowsInWorld = 7;
    public int colsInWorld = 7;

    public int startRow = 3;
    public int startCol = 3;

    public float splitChance = 0.3f; // 0.0 is never split, 1.0 is always split

    public float turnChanceModifier = 0.0f; // 0.0 is no modifier, 1.0 is always turn

    //------------//
    // Build Main //
    //------------//

    void Start()
    {
        //BuildNewWorld();
    }

    /// <summary>
    /// Builds a new world with the class's parameters and returns the start node of the world.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if world dimensions, start coordinates, or split chance is invalid.</exception>
    /// <returns>The start node in the new world.</returns>
    public LevelNode BuildNewWorld()
    {
        // Verify world dimensions
        if (rowsInWorld < 1 || colsInWorld < 1)
        {
            throw new InvalidOperationException(
                string.Format("Invalid world dimensions ({0}, {1}). Dimensions must be >= 1.", rowsInWorld, colsInWorld));
        }

        // Verify split chance
        if (splitChance < 0 || splitChance > 1)
        {
            throw new InvalidOperationException(
                string.Format("Invalid split chance {0}. Split chance must be >= 0 and <= 1.", splitChance));
        }

        // Create the world
        LevelWorld world = new LevelWorld(rowsInWorld, colsInWorld);

        // Verify start coordinates
        if (!AreCoordinatesInsideWorld(world, startRow, startCol))
        {
            throw new InvalidOperationException(
                string.Format("Invalid start coordinates ({0}, {1}) for world with dimensions ({0}, {1}). Coordinates must be located inside world.", startRow, startCol, rowsInWorld, colsInWorld));
        }

        // Add the first path and start node
        LevelPath firstPath = new LevelPath(world);
        LevelNode startNode = new LevelNode(firstPath, startRow, startCol);

        // Get the directions for the next two nodes
        List<Direction> possibleDirections = GetPossibleDirectionsForPathContinuation(firstPath);
        Debug.Assert(possibleDirections.Count >= 2); // Since the world is square and there are no other nodes, there should always be at least 2 possible directions

        Direction dirOfFirstPath = GetRandomDirection(possibleDirections);
        possibleDirections.Remove(dirOfFirstPath);
        Direction dirOfSecondPath = GetRandomDirection(possibleDirections);

        // Add the nodes to the world
        ProcessPathContinuation(firstPath, startNode, dirOfFirstPath);
        LevelPath secondPath = new LevelPath(world);
        ProcessPathContinuation(secondPath, startNode, dirOfSecondPath);

        // Begin adding paths
        List<LevelPath> currentActivePaths = new List<LevelPath>()
        {
            firstPath,
            secondPath
        };

        // Add preliminary paths
        BuildWorldFromExistingPaths(world, currentActivePaths);

        // Fill in empty nodes
        Tuple<int, int> emptyNodeCoordinates = GetCoordinatesOfEmptyNodeInWorld(world, true);
        while (emptyNodeCoordinates != null)
        {
            // Create the new path and node
            LevelPath newPath = new LevelPath(world);
            LevelNode newNode = new LevelNode(newPath, emptyNodeCoordinates.First, emptyNodeCoordinates.Second);

            // Connect it to an adjacent non-empty node
            LevelNode adjacentNode = GetAdjacentNonEmptyNode(world, newNode);
            if (adjacentNode == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot continue this path; node at ({0}, {1}) has no adjacent non-empty nodes.", newNode.WorldRow, newNode.WorldColumn));
            }

            // Get the direction to the adjacent node and connect the two
            Direction dirToAdjacentNode = GetDirectionToCoordinates(newNode.WorldRow, newNode.WorldColumn, adjacentNode.WorldRow, adjacentNode.WorldColumn);
            newNode.UpdateOpenSideStatus(dirToAdjacentNode, true);
            adjacentNode.UpdateOpenSideStatus(GetOppositeDirection(dirToAdjacentNode), true);

            BuildWorldFromExistingPaths(world, new List<LevelPath>() { newPath });
            emptyNodeCoordinates = GetCoordinatesOfEmptyNodeInWorld(world, true);
        }
        
        return startNode;
    }


    //---------------//
    // Build Helpers //
    //---------------//

    /// <summary>
    /// Continues building the world given the current active paths in it.
    /// </summary>
    /// <param name="world">The world to continue building.</param>
    /// <param name="currentActivePaths">The current active paths in the world.</param>
    /// <returns>The world after all active paths have completed.</returns>
    private LevelWorld BuildWorldFromExistingPaths(LevelWorld world, List<LevelPath> currentActivePaths)
    {
        while (currentActivePaths.Count > 0)
        {
            List<LevelPath> nextIterationActivePaths = new List<LevelPath>();
            foreach (var path in currentActivePaths)
            {
                // Verify that the path has at least one node already
                LevelNode tailNode = path.TailNode;
                Debug.Assert(tailNode != null);

                // Get the possible directions to move
                List<Direction> possibleDirections = GetPossibleDirectionsForPathContinuation(path);

                // Check to see if we're out of possible directions
                if (possibleDirections.Count == 0)
                {
                    // Note that we don't add this current path to the next iteration's paths; this path is done
                    continue;
                }

                // Continue the path in a random direction
                Direction dirToContinueIn = GetRandomDirection(possibleDirections);
                bool addPathToNextIteration = ProcessPathContinuation(path, tailNode, dirToContinueIn);
                if (addPathToNextIteration)
                {
                    nextIterationActivePaths.Add(path);
                }

                // Check for a split
                float splitCheck = UnityEngine.Random.Range(0.0f, 1.0f);
                if (splitCheck < splitChance)
                {
                    possibleDirections.Remove(dirToContinueIn);

                    // Check if there are any directions to continue in
                    if (possibleDirections.Count != 0)
                    {
                        // Get the direction for the new node
                        Direction dirToSplitIn = GetRandomDirection(possibleDirections);

                        // Create the new path and add a node to it
                        LevelPath splitPath = new LevelPath(world);
                        addPathToNextIteration = ProcessPathContinuation(splitPath, tailNode, dirToSplitIn);
                        if (addPathToNextIteration)
                        {
                            nextIterationActivePaths.Add(splitPath);
                        }
                    }
                }
            }

            // Update the current paths for the next iteration
            currentActivePaths = nextIterationActivePaths;
        }

        return world;
    }

    /// <summary>
    /// Finds and returns the coordinates of the first empty node in the given world.
    /// </summary>
    /// <param name="world">The world whose node to return.</param>
    /// <param name="mustBeAdjacentToNonEmptyNode">Whether or not the empty node must be adjacent to a non-empty node.</param>
    /// <returns>The coordinates of the empty node, or null if no node.</returns>
    private Tuple<int, int> GetCoordinatesOfEmptyNodeInWorld(LevelWorld world, bool mustBeAdjacentToNonEmptyNode = false)
    {
        for (int rr = 0; rr < world.ChildNodes.GetLength(0); rr++)
        {
            for (int cc = 0; cc < world.ChildNodes.GetLength(1); cc++)
            {
                LevelNode currNode = world.ChildNodes[rr, cc];
                if (currNode == null)
                {
                    LevelNode adjacentNonEmptyNode = GetAdjacentNonEmptyNode(world, rr, cc);
                    if (!mustBeAdjacentToNonEmptyNode || adjacentNonEmptyNode != null)
                    {
                        return new Tuple<int, int>(rr, cc);
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a randomly selected non-empty node adjacent to the given node.
    /// </summary>
    /// <param name="world">The world whose node to return.</param>
    /// <param name="node">The node whose adjacent node to return.</param>
    /// <returns>The adjacent, non-empty node, or null if no node.</returns>
    private LevelNode GetAdjacentNonEmptyNode(LevelWorld world, LevelNode node)
    {
        return GetAdjacentNonEmptyNode(world, node.WorldRow, node.WorldColumn);
    }

    /// <summary>
    /// Returns a randomly selected non-empty node adjacent to the node with the given coordinates.
    /// </summary>
    /// <param name="world">The world whose node to return.</param>
    /// <param name="rowOfNode">The row of the node whose adjacent node to return.</param>
    /// <param name="colOfNode">The column of the node whose adjacent node to return.</param>
    /// <returns>The adjacent, non-empty node, or null if no node.</returns>
    private LevelNode GetAdjacentNonEmptyNode(LevelWorld world, int rowOfNode, int colOfNode)
    {
        List<Direction> directions = new List<Direction>()
        {
            Direction.UP,
            Direction.RIGHT,
            Direction.DOWN,
            Direction.LEFT
        };

        List<LevelNode> adjacentNonEmptyNodes = new List<LevelNode>();

        // Find all non-empty adjacent nodes
        foreach (var dir in directions)
        {
            Tuple<int, int> coordinates = GetNewCoordinatesInDirection(rowOfNode, colOfNode, dir);
            int newRow = coordinates.First;
            int newCol = coordinates.Second;

            if (AreCoordinatesInsideWorld(world, newRow, newCol) && world.ChildNodes[newRow, newCol] != null)
            {
                adjacentNonEmptyNodes.Add(world.ChildNodes[newRow, newCol]);
            }
        }

        // Check if we have no adjacent nodes
        if (adjacentNonEmptyNodes.Count == 0)
        {
            return null;
        }

        // If we do, then return one at random
        int index = UnityEngine.Random.Range(0, adjacentNonEmptyNodes.Count);
        return adjacentNonEmptyNodes[index];
    }

    /// <summary>
    /// Returns a list of the possible directions the given path could continue in.
    /// </summary>
    /// <param name="path">The path whose directions to return.</param>
    /// <returns>The possible directions to continue in.</returns>
    private List<Direction> GetPossibleDirectionsForPathContinuation(LevelPath path)
    {
        List<Direction> directions = new List<Direction>()
        {
            Direction.UP,
            Direction.RIGHT,
            Direction.DOWN,
            Direction.LEFT
        };

        List<Direction> possibleDirectionsForPath = new List<Direction>();

        foreach (var dir in directions)
        {
            if (IsDirectionPossibleForPathContinuation(path, dir))
            {
                possibleDirectionsForPath.Add(dir);
            }
        }

        return possibleDirectionsForPath;
    }

    /// <summary>
    /// Returns whether or not the given path could continue in the given direction.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="dir">The direction to check.</param>
    /// <returns>True if the path could continue in that direction; false otherwise.</returns>
    private bool IsDirectionPossibleForPathContinuation(LevelPath path, Direction dir)
    {
        // Verify that the path has a tail node
        LevelNode tailNode = path.TailNode;
        if (tailNode == null)
        {
            throw new ArgumentException("Cannot operate on given path; path has no nodes.");
        }

        // Get the new coordinates
        Tuple<int, int> newCoordinates = GetNewCoordinatesInDirection(tailNode.WorldRow, tailNode.WorldColumn, dir);
        int newRow = newCoordinates.First;
        int newCol = newCoordinates.Second;

        // If the coordinates are outside the world, we can't continue here
        if (!AreCoordinatesInsideWorld(path.ParentWorld, newRow, newCol))
        {
            return false;
        }

        // If there is a non tail node at these coordinates, we can't continue here
        LevelNode nodeAtCoordinates = path.ParentWorld.ChildNodes[newRow, newCol];
        if (nodeAtCoordinates != null && !nodeAtCoordinates.IsTailNodeOfPath())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns whether or not the given coordinates lie inside the given world.
    /// </summary>
    /// <param name="world">The world to check.</param>
    /// <param name="row">The row portion of the coordinates to check.</param>
    /// <param name="col">The column portion of the coodinates to check.</param>
    /// <returns>True if the coodinates are inside the world; false otherwise.</returns>
    private bool AreCoordinatesInsideWorld(LevelWorld world, int row, int col)
    {
        // Check top and left
        if (row < 0 || col < 0)
        {
            return false;
        }

        // Check bottom and right
        if (row >= world.RowsInWorld || col >= world.ColumnsInWorld)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the new coordinates for the given node after moving one unit in the given direction.
    /// </summary>
    /// <param name="node">The node whose coordinates to use.</param>
    /// <param name="dir">The direction to move in.</param>
    /// <returns>The new coordinates.</returns>
    private Tuple<int, int> GetNewCoordinatesInDirection(LevelNode node, Direction dir)
    {
        return GetNewCoordinatesInDirection(node.WorldRow, node.WorldColumn, dir);
    }

    /// <summary>
    /// Gets the new coordinates from the given starting row and column after moving one unit in the given direction.
    /// </summary>
    /// <param name="startingRow">The row portion of the starting coordinates.</param>
    /// <param name="startingCol">The column portion of the starting coodinates.</param>
    /// <param name="dir">The direction to move in.</param>
    /// <exception cref="ArgumentException">Thrown if an invalid direction is passed in.</exception>
    /// <returns>The new coordinates.</returns>
    private Tuple<int, int> GetNewCoordinatesInDirection(int startingRow, int startingCol, Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
                return new Tuple<int, int>(startingRow - 1, startingCol);

            case Direction.RIGHT:
                return new Tuple<int, int>(startingRow, startingCol + 1);

            case Direction.DOWN:
                return new Tuple<int, int>(startingRow + 1, startingCol);

            case Direction.LEFT:
                return new Tuple<int, int>(startingRow, startingCol - 1);

            default:
                throw new ArgumentException(
                    string.Format("Invalid direction '{0}' passed to method; cannot determine new coordiantes.", dir));
        }
    }

    /// <summary>
    /// Gets a random direction from the given list of directions.
    /// </summary>
    /// <param name="dirs">The list of directions to get a direction from.</param>
    /// <exception cref="ArgumentException">Thrown if the list of dirs is empty.</exception>
    /// <returns>The randomly selected direction.</returns>
    private Direction GetRandomDirection(List<Direction> dirs)
    {
        // Verify that we have at least one direction to choose from
        if (dirs.Count == 0)
        {
            throw new ArgumentException("List of directions 'dirs' cannot be empty.");
        }

        int index = UnityEngine.Random.Range(0, dirs.Count - 1);
        return dirs[index];
    }

    /// <summary>
    /// Returns the opposite direction of the given direction; for example, given 'UP' this method returns DOWN.
    /// </summary>
    /// <param name="dir">The direction to get the opposited direction of.</param>
    /// <exception cref="ArgumentException">Thrown if an invalid direction is passed in.</exception>
    /// <returns>The opposite direction.</returns>
    private Direction GetOppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.UP:
                return Direction.DOWN;

            case Direction.RIGHT:
                return Direction.LEFT;

            case Direction.DOWN:
                return Direction.UP;

            case Direction.LEFT:
                return Direction.RIGHT;

            default:
                throw new ArgumentException(
                    string.Format("Invalid direction '{0}' passed to method; cannot get opposite direction.", dir));
        }
    }

    /// <summary>
    /// Infers the direction from one coordinate to another.
    /// </summary>
    /// <param name="rowOne">The row of the first coordinate.</param>
    /// <param name="colOne">The column of the first coordinate.</param>
    /// <param name="rowTwo">The row of the second coordinate.</param>
    /// <param name="colTwo">The column of the second coordinate.</param>
    /// <returns>The direction from the first coordinate to the second, or NONE if the coordinates are not adjacent.</returns>
    private Direction GetDirectionToCoordinates(int rowOne, int colOne, int rowTwo, int colTwo)
    {
        // If we are on the same row, then we must be either LEFT or RIGHT
        if (rowOne == rowTwo)
        {
            if (colOne == colTwo - 1)
            {
                return Direction.RIGHT;
            }
            else if (colOne == colTwo + 1)
            {
                return Direction.LEFT;
            }

            // Coordinates not adjacent
            return Direction.NONE;
        }

        // If we are on the same column, then we must be either UP or DOWN
        else if (colOne == colTwo)
        {
            if (rowOne == rowTwo - 1)
            {
                return Direction.DOWN;
            }
            else if (rowOne == rowTwo + 1)
            {
                return Direction.UP;
            }

            // Coordinates not adjacent
            return Direction.NONE;
        }

        // Coordinates not adjacent
        return Direction.NONE;
    }

    /// <summary>
    /// Continues the given path from the given node in the given direction.
    /// </summary>
    /// <param name="path">The path to continue.</param>
    /// <param name="previousNode">The previous node that connects to the new node or join. Typically, this will be the given path's tail node.</param>
    /// <param name="directionToContinue">The direction to continue the path in from the previous node.</param>
    /// <exception cref="InvalidOperationException">Thrown if the new node's coordinates would be outside the world.</exception>
    /// <returns>True if the path can continue after this step; false otherwise.</returns>
    private bool ProcessPathContinuation(LevelPath path, LevelNode previousNode, Direction directionToContinue)
    {
        Tuple<int, int> newNodeCoords = GetNewCoordinatesInDirection(previousNode, directionToContinue);
        bool pathContinuesNextIteration = false;

        // Verify that the new coordinates are inside the world
        if (!AreCoordinatesInsideWorld(previousNode.ParentWorld, newNodeCoords.First, newNodeCoords.Second))
        {
            throw new InvalidOperationException(string.Format(
                "Cannot add new node at the given direction from the given node. New node would have coordinates ({0}, {1}), which are outside the world.", newNodeCoords.First, newNodeCoords.Second));
        }

        // If there's no node at these coordinates, create a new node
        LevelNode nodeAtCoordinates = path.ParentWorld.ChildNodes[newNodeCoords.First, newNodeCoords.Second];
        if (nodeAtCoordinates == null)
        {
            nodeAtCoordinates = new LevelNode(path, newNodeCoords.First, newNodeCoords.Second);

            // Since we added a new node (and didn't join paths), this path continues
            pathContinuesNextIteration = true;
        }

        // Join the nodes
        previousNode.UpdateOpenSideStatus(directionToContinue, true);
        nodeAtCoordinates.UpdateOpenSideStatus(GetOppositeDirection(directionToContinue), true);

        return pathContinuesNextIteration;
    }
}
