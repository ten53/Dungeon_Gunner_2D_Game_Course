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
  /// Disable collision tilemap renderer
  /// </summary>
  private void DisableCollisionTilemapRenderer()
  {
    collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
  }

}
