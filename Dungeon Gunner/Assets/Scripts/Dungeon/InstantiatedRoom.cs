using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public class InstantiatedRoom : MonoBehaviour
{
  [HideInInspector] public Room room;
  [HideInInspector] public Grid grid;
  [HideInInspector] public Tilemap groundTilemap;
  [HideInInspector] public Tilemap decoration1Tilemap;
  [HideInInspector] public Tilemap decoration2Tilemap;
  [HideInInspector] public Tilemap frontTilemap;
  [HideInInspector] public Tilemap collisionTilemap;
  [HideInInspector] public Tilemap minimapTilemap;
  [HideInInspector] public Bounds roomColliderBounds;

  private BoxCollider2D boxCollider2D;

  void Awake()
  {
    boxCollider2D = GetComponent<BoxCollider2D>();

    // Save room collider bounds
    roomColliderBounds = boxCollider2D.bounds;
  }

  /// <summary>
  /// Initialize the instantiated room
  /// </summary>
  public void Initialize(GameObject roomGameObject)
  {
    PopulateTilemapMemberVariables(roomGameObject);

    BlockOffUnusedDoorWays();

    DisableCollisionTilemapRenderer();
  }

  /// <summary>
  /// Populate the tilemap and grid member variables
  /// </summary>
  private void PopulateTilemapMemberVariables(GameObject roomGameObject)
  {
    // Get grid component
    grid = roomGameObject.GetComponentInChildren<Grid>();

    // Get tilemaps in children
    Tilemap[] tilemaps = roomGameObject.GetComponentsInChildren<Tilemap>();

    foreach (Tilemap tilemap in tilemaps)
    {
      if (tilemap.gameObject.tag == "groundTilemap")
      {
        groundTilemap = tilemap;
      }
      else if (tilemap.gameObject.tag == "decoration1Tilemap")
      {
        decoration1Tilemap = tilemap;
      }
      else if (tilemap.gameObject.tag == "decoration2Tilemap")
      {
        decoration2Tilemap = tilemap;
      }
      else if (tilemap.gameObject.tag == "frontTilemap")
      {
        frontTilemap = tilemap;
      }
      else if (tilemap.gameObject.tag == "collisionTilemap")
      {
        collisionTilemap = tilemap;
      }
      else if (tilemap.gameObject.tag == "minimapTilemap")
      {
        minimapTilemap = tilemap;
      }
    }
  }

  /// <summary>
  /// Block off unused doorways in the room
  /// </summary>
  public void BlockOffUnusedDoorWays()
  {
    // Loop through all doorways
    foreach (Doorway doorway in room.doorWayList)
    {
      if (doorway.isConnected) continue;

      // Block unconnected doorways using tiles on tilemaps
      if (collisionTilemap != null)
      {
        BlockDoorwayOnTilemapLayer(collisionTilemap, doorway);
      }

      if (minimapTilemap != null)
      {
        BlockDoorwayOnTilemapLayer(minimapTilemap, doorway);
      }

      if (groundTilemap != null)
      {
        BlockDoorwayOnTilemapLayer(groundTilemap, doorway);
      }

      if (decoration1Tilemap != null)
      {
        BlockDoorwayOnTilemapLayer(decoration1Tilemap, doorway);
      }

      if (decoration2Tilemap != null)
      {
        BlockDoorwayOnTilemapLayer(decoration2Tilemap, doorway);
      }

      if (frontTilemap != null)
      {
        BlockDoorwayOnTilemapLayer(frontTilemap, doorway);
      }
    }
  }

  /// <summary>
  /// Block a doorway on the tilemap layer
  /// </summary>
  private void BlockDoorwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
  {
    switch (doorway.orientation)
    {
      case Orientation.north:
      case Orientation.south:
        BlockDoorwayHorizontally(tilemap, doorway);
        break;

      case Orientation.east:
      case Orientation.west:
        BlockDoorwayVertically(tilemap, doorway);
        break;

      case Orientation.none:
        break;

      default:
        break;
    }
  }

  /// <summary>
  /// Block doorays horizontally  - for north and south doorways
  /// </summary>
  public void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
  {
    Vector2Int startPosition = doorway.doorwayStartCopyPosition;

    // Loop through all tiles to copy
    for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
    {
      for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
      {
        // Get rotation of tile being copied
        Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

        // Copy tile
        tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

        // Set rotation of tile copied
        tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
      }
    }
  }

  /// <summary>
  /// Block doorays vertically  - for east and west doorways
  /// </summary>
  public void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
  {
    Vector2Int startPosition = doorway.doorwayStartCopyPosition;

    // Loop through all tiles to copy
    for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
    {
      for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
      {
        // Get rotation of tile being copied
        Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

        // Copy tile
        tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

        // Set rotation of tile copied
        tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
      }
    }
  }

  /// <summary>
  /// Disable collision tilemap renderer
  /// </summary>
  private void DisableCollisionTilemapRenderer()
  {
    collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
  }

}
