using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehavior<DungeonBuilder>
{
  public Dictionary<string, Room> dungeonBuilderRoomDictionary = new Dictionary<string, Room>();
  private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
  private List<RoomTemplateSO> roomTemplateList = null;
  private RoomNodeTypeListSO roomNodeTypeList;
  private bool dungeonBuildSuccessful;

  protected override void Awake()
  {
    base.Awake();

    // Load the room node type list
    LoadRoomNodeTypeList();

    // Set dimmed material to fully visible
    GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
  }

  /// <summary>
  /// Load the room node type list
  /// </summary>
  private void LoadRoomNodeTypeList()
  {
    roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
  }

  /// <summary>
  /// Generate random dungeon, return true if dungeon built, false if failed
  /// </summary>
  public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
  {
    roomTemplateList = currentDungeonLevel.roomTemplateList;

    // Load the scriptable object room templates into the dictionary
    LoadRoomTemplatesIntoDictionary();

    dungeonBuildSuccessful = false;
    int dungeonBuildAttempts = 0;

    while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
    {
      dungeonBuildAttempts++;

      // Select a random room node graph from the list
      RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

      int dungeonRebuildAttemptsForNodeGraph = 0;
      dungeonBuildSuccessful = false;

      // Loop until dungeon successfully built or more than max attempts for node graph
      while (!dungeonBuildSuccessful && dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
      {
        // Clear dungeon room gameobjects and dungeon room dictionary
        ClearDungeon();

        dungeonRebuildAttemptsForNodeGraph++;

        // Attempt to build a random dungeon for the selected room node graph
        dungeonBuildSuccessful = AttemptToBuildRandomDungeon(roomNodeGraph);
      }

      if (dungeonBuildSuccessful)
      {
        // Instantiate Room GameObjects
        InstantiateRoomGameObjects();
      }
    }
    return dungeonBuildSuccessful;
  }

  /// <summary>
  /// Load the room templates into the dictionary
  /// </summary>
  private void LoadRoomTemplatesIntoDictionary()
  {
    // Clear room template dictionary
    roomTemplateDictionary.Clear();

    // Load room template dictionary
    foreach (RoomTemplateSO roomTemplate in roomTemplateList)
    {
      if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
      {
        roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
      }
      else
      {
        Debug.Log($"Duplicate Room Template Key In {roomTemplateList}");
      }
    }
  }


  /// <summary>
  /// Attempt to randomly build the dungeon for the specified room node graph. Returns true if
  /// a successful random layout was generated, else return false if a problem was encountered and
  /// another attempt is required.
  /// </summary>
  private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
  {
    // Create open room nodde queue
    Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

    // Add entrance node to room node queue from room node graph
    RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

    if (entranceNode != null)
    {
      openRoomNodeQueue.Enqueue(entranceNode);
    }
    else
    {
      Debug.Log("No entrance node");
      return false; // dungeon not built
    }

    // Start with no room overlaps
    bool noRoomOverlaps = true;

    // Process open room nodes queue
    noRoomOverlaps = ProcessRoomsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

    // If all the room nodes have been processed and there hasn't been a room overlap then return true
    if (openRoomNodeQueue.Count == 0 && noRoomOverlaps)
    {
      return true;
    }
    else
    {
      return false;
    }

  }

  /// <summary>
  /// Process room in the open room node queue, returning true if there are no room overlaps
  /// </summary>
  private bool ProcessRoomsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
  {
    // While room nodes in open room node queue & no room overlaps detected
    while (openRoomNodeQueue.Count > 0 && noRoomOverlaps == true)
    {
      // Get next room node from open room node queue
      RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

      // Add child nodes to queue from room node graph (with liks to parent room)
      foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
      {
        openRoomNodeQueue.Enqueue(childRoomNode);
      }

      // if the room is the entrance mark as positioned and add to room dictionary
      if (roomNode.roomNodeType.isEntrance)
      {
        RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);

        Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

        room.isPositioned = true;

        // Add room to room dictionary
        dungeonBuilderRoomDictionary.Add(room.id, room);
      }
      // else if room type isn't an entrance
      else
      {
        // Get parent room for node
        Room parentRoom = dungeonBuilderRoomDictionary[roomNode.parentRoomNodeIDList[0]];

        // See if room can be placed without overlaps
        noRoomOverlaps = CanPlaceRoomWithNodeOverlaps(roomNode, parentRoom);
      }
    }

    return noRoomOverlaps;
  }

  /// <summary>
  /// Attempt to place the room node in the dungeon - if room can be place return the room, else return null;
  /// </summary>
  private bool CanPlaceRoomWithNodeOverlaps(RoomNodeSO roomNode, Room parentRoom)
  {
    // Initialize and assume overlap until proven otherwise
    bool roomOverlaps = true;


    // Do while room ovelaps - try to place against all available doorways of parent until 
    // the room is successfully placed without overlap.
    while (roomOverlaps)
    {
      // Select random unconnected available doorway for parent
      List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();

      if (unconnectedAvailableParentDoorways.Count == 0)
      {
        // If no more doorways to try, then overlap failure.
        return false; // room overlaps;
      }

      Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

      // Get a random template for room node that is consistent with the parent door orientation
      RoomTemplateSO roomTemplate = GetRandomRoomTemplateForRoomConsitentWithParent(roomNode, doorwayParent);

      // Create a room
      Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

      // Place the room - return true if the room doesn't overlap
      if (PlaceTheRoom(parentRoom, doorwayParent, room))
      {

      }

    }

  }

  /// <summary>
  /// Get random room template for room node taking into account the parent doorway orientation
  /// </summary>
  private RoomTemplateSO GetRandomRoomTemplateForRoomConsitentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
  {
    RoomTemplateSO roomTemplate = null;

    // If room node is a corridor then select random correct corridor room template based on parent doorway orientation
    if (roomNode.roomNodeType.isCorridor)
    {
      switch (doorwayParent.orientation)
      {
        case Orientation.north:
        case Orientation.south:
          roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
          break;

        case Orientation.east:
        case Orientation.west:
          roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
          break;

        case Orientation.none:
          break;

        default:
          break;
      }
    }
    // Select random room template
    else
    {
      roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
    }

    return roomTemplate;
  }

  /// <summary>
  /// Place the room - return true if the room doesn't overlap, false if it does
  /// </summary>
  private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
  {
    // Get current room doorway position
    Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

    // Return false if no doorway in room opposite of parent doorway
    if (doorway == null)
    {
      // Mark the parent doorway as unavailable so we don't try to connect it again
      doorwayParent.isUnavailable = true;
      return false;
    }

    // Calculate 'world' grid parent doorway position
    Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

    Vector2Int adjustment = Vector2Int.zero;

    // Calculate adjustment position offset based on room doorway position that we are trying to connect
    // e.g. if this doorway is west then we need to add (1, 0) to the east parent doorway
    switch (doorway.orientation)
    {
      case Orientation.north:
        adjustment = new Vector2Int(0, -1);
        break;

      case Orientation.east:
        adjustment = new Vector2Int(-1, 0);
        break;

      case Orientation.south:
        adjustment = new Vector2Int(0, 1);
        break;

      case Orientation.west:
        adjustment = new Vector2Int(1, 0);
        break;

      case Orientation.none:
        break;

      default:
        break;
    }
  }

  /// <summary>
  /// Get the doorway from the doorway list that has the opposite orientation to doorway
  /// </summary>
  private Doorway GetOppositeDoorway(Doorway parentDoorway, List<Doorway> doorwayList)
  {
    foreach (Doorway doorwayToCheck in doorwayList)
    {
      if (parentDoorway.orientation == Orientation.east && doorwayToCheck.orientation == Orientation.west)
      {
        return doorwayToCheck;
      }
      else if (parentDoorway.orientation == Orientation.west && doorwayToCheck.orientation == Orientation.east)
      {
        return doorwayToCheck;
      }
      else if (parentDoorway.orientation == Orientation.north && doorwayToCheck.orientation == Orientation.south)
      {
        return doorwayToCheck;
      }
      else if (parentDoorway.orientation == Orientation.south && doorwayToCheck.orientation == Orientation.north)
      {
        return doorwayToCheck;
      }
    }

    return null;
  }

  /// <summary>
  /// Get a random room template from the room template list that matches the room type and return it,
  /// return null if no matching room templates found
  /// </summary>
  private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
  {
    List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

    // Loop through template list
    foreach (RoomTemplateSO roomTemplate in roomTemplateList)
    {
      // Add matching room templates
      if (roomTemplate.roomNodeType == roomNodeType)
      {
        matchingRoomTemplateList.Add(roomTemplate);
      }
    }

    // Return null if list is empty
    if (matchingRoomTemplateList.Count == 0)
    {
      return null;
    }

    // Select random room template from list and return
    return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
  }

  /// <summary>
  /// Get unconnected doorways
  /// </summary>
  private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
  {
    // Loop through doorway list
    foreach (Doorway doorway in roomDoorwayList)
    {
      if (!doorway.isConnected && !doorway.isUnavailable)
      {
        yield return doorway;
      }
    }
  }

  /// <summary>
  /// Create room based on room template and layout node, and return created room
  /// </summary>
  private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
  {
    // Initialize a room from template
    Room room = new Room();

    room.templateID = roomTemplate.guid;
    room.id = roomNode.id;
    room.prefab = roomTemplate.prefab;
    room.roomNodeType = roomTemplate.roomNodeType;
    room.lowerBounds = roomTemplate.lowerBounds;
    room.upperBounds = roomTemplate.upperBounds;
    room.spawnPositionArray = roomTemplate.spawnPositionArray;
    room.templateLowerBounds = roomTemplate.lowerBounds;
    room.templateUpperBounds = roomTemplate.upperBounds;
    room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
    room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

    // Set parent ID for room
    if (roomNode.parentRoomNodeIDList.Count == 0) // Entrance
    {
      room.parentRoomID = "";
      room.isPreviouslyVisited = true;
    }
    else
    {
      room.parentRoomID = roomNode.parentRoomNodeIDList[0];
    }

    return room;
  }

  /// <summary>
  /// Select a random room node graph from the list of room node graphs
  /// </summary>
  private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
  {
    if (roomNodeGraphList.Count > 0)
    {
      return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
    }
    else
    {
      Debug.Log($"No node room in graph list");
      return null;
    }
  }

  /// <summary>
  /// Create a deep copy of doorway list
  /// </summary>
  private List<Doorway> CopyDoorwayList(List<Doorway> oldDoorwayList)
  {
    List<Doorway> newDoorwayList = new List<Doorway>();

    foreach (Doorway doorway in oldDoorwayList)
    {
      Doorway newDoorway = new Doorway();

      newDoorway.position = doorway.position;
      newDoorway.orientation = doorway.orientation;
      newDoorway.doorPrefab = doorway.doorPrefab;
      newDoorway.isConnected = doorway.isConnected;
      newDoorway.isUnavailable = doorway.isUnavailable;
      newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
      newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;
      newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;

      newDoorwayList.Add(newDoorway);
    }

    return newDoorwayList;
  }

  /// <summary>
  /// Create a deep copy of string list
  /// </summary>
  private List<string> CopyStringList(List<string> oldStringList)
  {
    List<string> newStringList = new List<string>();

    foreach (string stringValue in oldStringList)
    {
      newStringList.Add(stringValue);
    }

    return newStringList;
  }

  /// <summary>
  /// Clear dungeon room gameobjects and dungeon room dictionary
  /// </summary>
  private void ClearDungeon()
  {
    // Destroy instantiated dungeon object and clear dungeon manager room dictionary
    if (dungeonBuilderRoomDictionary.Count > 0)
    {
      foreach (KeyValuePair<string, Room> keyValuePair in dungeonBuilderRoomDictionary)
      {
        Room room = keyValuePair.Value;
        if (room.instantiatedRoom != null)
        {
          Destroy(room.instantiatedRoom.gameObject);
        }
      }
      dungeonBuilderRoomDictionary.Clear();
    }
  }

}
