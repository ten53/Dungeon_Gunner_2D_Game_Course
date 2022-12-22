using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject
{
  [HideInInspector]
  public RoomNodeTypeListSO roomNodeTypeList;

  [HideInInspector]
  public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();

  [HideInInspector]
  // string key is going to be a unique id (guid)
  public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

  private void Awake()
  {
    LoadRoomNodeDictionary();
  }

  /// <summary>
  /// Load the room node dictionary from the room node list
  /// </summary>
  private void LoadRoomNodeDictionary()
  {
    roomNodeDictionary.Clear();

    // Populate dictionary
    foreach (RoomNodeSO node in roomNodeList)
    {
      roomNodeDictionary[node.id] = node;
    }
  }

  /// <summary>
  /// Get room node ny nodeID
  /// </summary>
  public RoomNodeSO GetRoomNode(string roomeNodeID)
  {
    if (roomNodeDictionary.TryGetValue(roomeNodeID, out RoomNodeSO roomNode))
    {
      return roomNode;
    }
    return null;
  }


  // The following should only be run in the Unity Editor
  #region Editor Code

#if UNITY_EDITOR

  [HideInInspector]
  public RoomNodeSO roomNodeToDrawLineFrom = null;

  [HideInInspector]
  public Vector2 linePosition;

  // Repopulate node dictonary every time a change is made in the editor
  public void OnValidate()
  {
    LoadRoomNodeDictionary();
  }

  public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position)
  {
    roomNodeToDrawLineFrom = node;
    linePosition = position;
  }

#endif

  #endregion Editor Code



}
