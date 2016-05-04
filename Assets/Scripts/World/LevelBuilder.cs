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

    public float splitChance = 0.3f;


    //------------//
    // Build Main //
    //------------//

    void Start()
    {
        BuildNewWorld();
    }

    /// <summary>
    /// Builds a new world with the class's parameters and returns the start node of the world.
    /// </summary>
    /// <returns>The start node in the new world.</returns>
    public LevelNode BuildNewWorld()
    {
        LevelWorld world = new LevelWorld(rowsInWorld, colsInWorld);

        // Add the first path and node
        LevelPath firstPath = new LevelPath(world);
        LevelNode startNode = new LevelNode(firstPath, startRow, startCol);

        // Add the next node on the first path
        List<Direction> possibleDirections = GetDirectionsNotOffWorld(firstPath);
        Direction firstDirToMove = GetRandomDirection(possibleDirections);

        Tuple<int, int> coordinatesForSecondNode = GetNewCoordinatesInDirection(startNode, firstDirToMove);
        LevelNode secondNode = new LevelNode(firstPath, coordinatesForSecondNode.First, coordinatesForSecondNode.Second);

        startNode.UpdateOpenSideStatus(firstDirToMove, true);
        secondNode.UpdateOpenSideStatus(GetOppositeDirection(firstDirToMove), true);

        // Add the second path and it's first node
        LevelPath secondPath = new LevelPath(world);

        List<Direction> possibleOtherDirections = new List<Direction>(possibleDirections);
        possibleOtherDirections.Remove(firstDirToMove); 
        Debug.Assert(possibleOtherDirections.Count > 0); // This should always contain at least 1 element
        Direction startDirOfSecondPath = GetRandomDirection(possibleOtherDirections);

        Tuple<int, int> coordinatesForThirdNode = GetNewCoordinatesInDirection(startNode, startDirOfSecondPath);
        LevelNode thirdNode = new LevelNode(secondPath, coordinatesForThirdNode.First, coordinatesForThirdNode.Second);

        startNode.UpdateOpenSideStatus(startDirOfSecondPath, true);
        thirdNode.UpdateOpenSideStatus(GetOppositeDirection(startDirOfSecondPath), true);

        // Begin adding paths
        List<LevelPath> currentActivePaths = new List<LevelPath>()
        {
            firstPath,
            secondPath
        };

        while (currentActivePaths.Count > 0)
        {
            List<LevelPath> nextIterationActivePaths = new List<LevelPath>();
            foreach (var path in currentActivePaths)
            {
                // Get the possible directions to move
                possibleDirections = GetDirectionsNotOffWorld(path);
                List<Direction> possibleDirectionsCopy = new List<Direction>(possibleDirections); // Copy so we can mutate possibleDirections

                // Check each direction for path conflicts
                foreach (var dir in possibleDirectionsCopy)
                {
                    Tuple<int, int> coords = GetNewCoordinatesInDirection(path.TailNode, dir);
                    LevelNode nodeAtLocation = world.ChildNodes[coords.First, coords.Second];

                    // If the node at that current location exists and it's not a tail node, we can't continue in this direction
                    if (nodeAtLocation != null && !nodeAtLocation.IsTailNodeOfPath())
                    {
                        possibleDirections.Remove(dir);
                    }
                }

                // Check to see if we're out of possible directions
                if (possibleDirections.Count == 0)
                {
                    continue;
                }

                // Make the new node
                Direction dirToContinueIn = GetRandomDirection(possibleDirections);
                Tuple<int, int> newCoordinates = GetNewCoordinatesInDirection(path.TailNode, dirToContinueIn);
                LevelNode newNode = new LevelNode(path, newCoordinates.First, newCoordinates.Second);

                Debug.Assert(path.ChildNodes.Count >= 2); // There should always be at least two nodes in a path after the last node was added
                path.ChildNodes[path.ChildNodes.Count - 2].UpdateOpenSideStatus(dirToContinueIn, true); 
                path.TailNode.UpdateOpenSideStatus(GetOppositeDirection(dirToContinueIn), true);
            }

            // Update the current paths for the next iteration
            currentActivePaths = nextIterationActivePaths;
        }
        

        return startNode;
    }


    //---------------//
    // Build Helpers //
    //---------------//

    /// <summary>
    /// Returns a list of the possible directions the given path could continue in without going off the world.
    /// </summary>
    /// <param name="path">The path whose directions to return.</param>
    /// <returns>The possible directions to continue in.</returns>
    private List<Direction> GetDirectionsNotOffWorld(LevelPath path)
    {
        List<Direction> directions = new List<Direction>()
        {
            Direction.UP,
            Direction.RIGHT,
            Direction.DOWN,
            Direction.LEFT
        };

        List<Direction> possibleDirectionsToMove = new List<Direction>();

        foreach (var dir in directions)
        {
            if (IsDirectionNotOffWorld(path, dir))
            {
                possibleDirectionsToMove.Add(dir);
            }
        }

        return possibleDirectionsToMove;
    }

    /// <summary>
    /// Returns whether or not the given path could continue in the given direction without going off the world.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="dir">The direction to check.</param>
    /// <returns>True if the path could continue in that direction; false otherwise.</returns>
    private bool IsDirectionNotOffWorld(LevelPath path, Direction dir)
    {
        // Make sure the the path has a tail node
        LevelNode tailNode = path.TailNode;
        if (tailNode == null)
        {
            throw new ArgumentException("Cannot operate on given path; path has no nodes.");
        }

        // Get the new coordinates
        Tuple<int, int> newCoordinates = GetNewCoordinatesInDirection(tailNode.WorldRow, tailNode.WorldColumn, dir);
        int newRow = newCoordinates.First;
        int newCol = newCoordinates.Second;

        return AreCoordinatesInsideWorld(path.ParentWorld, newRow, newCol);
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
}
