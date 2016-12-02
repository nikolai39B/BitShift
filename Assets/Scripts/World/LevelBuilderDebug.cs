using UnityEngine;
using System.Collections.Generic;

public class LevelBuilderDebug : MonoBehaviour
{
    public GameObject wall;
    public GameObject tile;

    public LevelWorld world;

    // Use this for initialization
    void Start()
    {
        LevelBuilder builder = GetComponent<LevelBuilder>();
        LevelNode startNode = builder.BuildNewWorld();
        world = startNode.ParentWorld;

        Dictionary<Direction, Quaternion> directions = new Dictionary<Direction, Quaternion>()
        {
            { Direction.UP, Quaternion.Euler(0, 0, 0) },
            { Direction.RIGHT, Quaternion.Euler(0, 0, 270) },
            { Direction.DOWN, Quaternion.Euler(0, 0, 180) },
            { Direction.LEFT, Quaternion.Euler(0, 0, 90) }
        };

        float rowOffset = world.RowsInWorld / 2.0f;
        float colOffset = world.ColumnsInWorld / 2.0f;

        for (int rr = 0; rr < world.RowsInWorld; rr++)
        {
            for (int cc = 0; cc < world.ColumnsInWorld; cc++)
            {
                LevelNode currNode = world.ChildNodes[rr, cc];

                if (currNode == null)
                {
                    Instantiate(tile, new Vector3(cc - colOffset, -1 * (rr - rowOffset)), Quaternion.identity);
                }

                else
                {
                    foreach (var dir in directions.Keys)
                    {
                        if (!currNode.OpenSides.Contains(dir))
                        {
                            Instantiate(wall, new Vector3(cc - colOffset, -1 * (rr - rowOffset)), directions[dir]);
                        }
                    }
                }
            }
        }
	}
}
