//using UnityEngine;
//using System;
//using System.Collections;

//public class GameGeneratorDebug : MonoBehaviour
//{
//    public GameObject blueTile;
//    public GameObject greenTile;
//    public GameObject redTile;
//    public GameObject yellowTile;

//    /// <summary>
//    /// Instantiates a tileset in the world to crudely represent the given world.
//    /// </summary>
//    /// <param name="world">The world to generate a tileset for.</param>
//    /// <param name="centerX">The x-coordinate at which to center the tiles.</param>
//    /// <param name="centerY">The y-coordinate at which to center the tiles.</param>
//    public void CreateTilesetFromWorld(World world, int centerX = 0, int centerY = 0)
//    {
//        // The x and y offsets are the conversion from the World coordinated to the Unity scene coordinates.
//        // As an example, if the offsets are both 0, then (0, 0) in World coordinates map to 
//        float offsetFromColToX = centerX - world.NumCols / 2.0f;
//        float offsetFromRowToY = centerY - world.NumRows / 2.0f;

//        // Create the backdrop
//        GameObject backdrop = (GameObject)Instantiate(blueTile, new Vector3(centerX, centerY), Quaternion.identity);
//        backdrop.transform.localScale = new Vector3(world.NumCols, world.NumRows, backdrop.transform.localScale.z);

//        // Create the tiles
//        for (int row = 0; row < world.NumRows; row++)
//        {
//            for (int col = 0; col < world.NumCols; col++)
//            {
//                PathNode node = world.GetNodeAtLocation(row, col);
//                GameObject tileToInstantiate = null;

//                // If there's a node here, get the tile based on node role
//                if (node != null)
//                {
//                    switch (node.NodeRole)
//                    {
//                        case PathNode.Role.START:
//                            tileToInstantiate = greenTile;
//                            break;

//                        case PathNode.Role.NORMAL:
//                            tileToInstantiate = yellowTile;
//                            break;

//                        case PathNode.Role.END:
//                            tileToInstantiate = redTile;
//                            break;

//                        default:
//                            tileToInstantiate = null;
//                            break;
//                    }
//                }

//                // Create the tile
//                if (tileToInstantiate != null)
//                {
//                    Instantiate(tileToInstantiate, new Vector3(col + offsetFromColToX, row + offsetFromRowToY), Quaternion.identity);
//                }
//            }
//        }
//    }
//}
