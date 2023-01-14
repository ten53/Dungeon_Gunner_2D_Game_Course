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
    }
  }

  /// <summary>
  /// Get a random room template from the room template list that matches the room type and return it,
  /// return null if no matching room templates found
  /// </summary>
  private RoomTemplateSO GetRoomTemplate(RoomNodeTypeSO roomNodeType)
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
