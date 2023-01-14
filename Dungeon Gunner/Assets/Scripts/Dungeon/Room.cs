using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
  public string id;
  public string templateID;
  public GameObject prefab;
  public RoomNodeTypeSO roomNodeType;
  public Vector2Int lowerBounds;
  public Vector2Int upperBounds;
  public Vector2Int templateLowerBounds;
  public Vector2Int templateUpperBounds;
  public Vector2Int[] spawnPositionArray;
  public List<string> childRoomIDList;
  public List<Doorway> doorWayList;
  public string parentRoomID;
  public bool isPositioned = true;
  public InstantiatedRoom instantiatedRoom;
  public bool isLit = false;
  public bool isClearedOfEnemeies;
  public bool isPreviouslyVisited = false;

  public Room()
  {
    childRoomIDList = new List<string>();
    doorWayList = new List<Doorway>();
  }

}
