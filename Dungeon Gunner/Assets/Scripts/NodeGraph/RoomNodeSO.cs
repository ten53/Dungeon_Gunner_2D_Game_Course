using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
  [HideInInspector]
  public string id;

  // Todo: Check if public vars can be private with [serializedField]

  [HideInInspector]
  public List<string> parentRoomNodeIDList = new List<string>();

  [HideInInspector]
  public List<string> childRoomNodeIDList = new List<string>();

  [HideInInspector]
  public RoomNodeGraphSO roomNodeGraph;

  [HideInInspector]
  public RoomNodeTypeListSO roomNodeTypeList;

  public RoomNodeTypeSO roomNodeType;



  // the following code should only be run in the editor
  #region Editor Code

#if UNITY_EDITOR

  [HideInInspector]
  public Rect rect;

  [HideInInspector]
  public bool isLeftClickDragging = false;

  [HideInInspector]
  public bool isSelected = false;

  /// <summary>
  /// Initialize node
  /// </summary>
  public void Initialize(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
  {
    this.rect = rect;
    this.id = Guid.NewGuid().ToString();
    this.name = "RoomNode";
    this.roomNodeGraph = nodeGraph;
    this.roomNodeType = roomNodeType;

    // Load room node type list
    roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
  }

  /// <summary>
  /// Draw node with the nodeStyle
  /// </summary>
  public void Draw(GUIStyle nodeStyle)
  {
    // Draw node box using begin area
    GUILayout.BeginArea(rect, nodeStyle);

    // Start region to detect popup selection changes
    EditorGUI.BeginChangeCheck();

    // If the room node has a parent or is of type entrance then display a label else display a popup
    if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
    {
      // Display label that can't be changed
      EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
    }
    else
    {
      // Display a popup using the roomNodeType name values that can be selected from (default lto the currently set roomNodeType)
      int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);

      int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

      roomNodeType = roomNodeTypeList.list[selection];
    }



    if (EditorGUI.EndChangeCheck())
    {
      EditorUtility.SetDirty(this);
    }

    GUILayout.EndArea();
  }

  /// <summary>
  /// Populate a string array with the room node types to display that can be selected
  /// </summary>
  public string[] GetRoomNodeTypesToDisplay()
  {
    string[] roomArray = new string[roomNodeTypeList.list.Count];

    for (int i = 0; i < roomNodeTypeList.list.Count; i++)
    {
      if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
      {
        roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
      }
    }

    return roomArray;
  }

  /// <summary>
  /// Process events for the code
  /// </summary>
  public void ProcessEvents(Event currentEvent)
  {
    switch (currentEvent.type)
    {
      // Process mouse down events
      case EventType.MouseDown:
        ProcessMouseDownEvent(currentEvent);
        break;

      // Process mouse up events
      case EventType.MouseUp:
        ProcessMouseUpEvent(currentEvent);
        break;

      // Process mouse drag events
      case EventType.MouseDrag:
        ProcessMouseDragEvent(currentEvent);
        break;

      default:
        break;
    }
  }

  /// <summary>
  /// Process mouse down event
  /// </summary>
  private void ProcessMouseDownEvent(Event currentEvent)
  {
    // left click down
    if (currentEvent.button == 0)
    {
      ProcessLeftClickDownEvent();
    }
    // right click down
    else if (currentEvent.button == 1)
    {
      ProcessRightClickDownEvent(currentEvent);
    }

  }

  /// <summary>
  /// Process left click down event
  /// </summary>
  private void ProcessLeftClickDownEvent()
  {
    Selection.activeObject = this;

    // Toggle node selection
    if (isSelected == true)
    {
      isSelected = false;
    }
    else
    {
      isSelected = true;
    }

    // Shorter version: isSelected = !isSelected;
  }

  /// <summary>
  /// Process mouse up event
  /// </summary>
  private void ProcessMouseUpEvent(Event currentEvent)
  {
    // If left click up
    if (currentEvent.button == 0)
    {
      ProcessLeftClickUpEvent();
    }
  }

  /// <summary>
  /// Process left click up event
  /// </summary>
  private void ProcessLeftClickUpEvent()
  {
    if (isLeftClickDragging)
    {
      isLeftClickDragging = false;
    }
  }

  /// <summary>
  /// Process right click down
  /// </summary>
  private void ProcessRightClickDownEvent(Event currentEvent)
  {
    roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
  }

  /// <summary>
  /// Process mouse drag event
  /// </summary>
  private void ProcessMouseDragEvent(Event currentEvent)
  {
    // process left click drag event
    if (currentEvent.button == 0)
    {
      ProcessLeftMouseDragEvent(currentEvent);
    }
  }

  /// <summary>
  /// Process left mouse drag event
  /// </summary>
  private void ProcessLeftMouseDragEvent(Event currentEvent)
  {
    isLeftClickDragging = true;

    DragNode(currentEvent.delta);
    GUI.changed = true;
  }

  /// <summary>
  /// Drag Node
  /// </summary>
  private void DragNode(Vector2 delta)
  {
    rect.position += delta;
    EditorUtility.SetDirty(this);
  }

  /// <summary>
  /// Add childID to the node (returns true if the node has been added, false otherwise)
  /// </summary>
  public bool AddChildRoomNodeIDToRoomNode(string childID)
  {
    // Check child node can be added validly to parent
    if (IsChildRoomValid(childID))
    {
      childRoomNodeIDList.Add(childID);
      return true;
    }

    return false;
  }

  /// <summary>
  /// Check the child node can be validly (see Settings) added to the paret node - return true if it can otherwise return false
  /// </summary>
  public bool IsChildRoomValid(string childID)
  {
    bool isConnectedBossNodeAlready = false;
    // Check if there is a boss room already connected in the node room graph
    foreach (RoomNodeSO roomNode in roomNodeGraph.roomNodeList)
    {
      if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
      {
        isConnectedBossNodeAlready = true;
      }
    }

    // if the child node has a type of boss room and there is already a connected boss room then return false
    if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
    {
      return false;
    }

    // If the cjild node has a type of none then return false
    if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isNone)
    {
      return false;
    }

    // If the node already has a child with this child ID return false
    if (childRoomNodeIDList.Contains(childID))
    {
      return false;
    }

    // If this node ID and the child ID are the same then return false
    if (id == childID)
    {
      return false;
    }

    // If this childID is already  in the parentID list return false
    if (parentRoomNodeIDList.Contains(childID))
    {
      return false;
    }

    // If the child node already has a parent return false
    if (roomNodeGraph.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
    {
      return false;
    }

    // If a child is a corridor and this node is a corridor then return false
    if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
    {
      return false;
    }

    //If child is not a corridor and this node is not a corridor (both are rooms) then return false
    if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
    {
      return false;
    }

    // If adding a corridor check that this node has less than the maximum permitted child coridors
    if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors)
    {
      return false;
    }

    // If the child room node is an entrance return false - the entrance must always be the top level parent node - cannot connect any node to it!
    if (roomNodeGraph.GetRoomNode(childID).roomNodeType.isEntrance)
    {
      return false;
    }

    // If adding a room to a corridor check that this corridor node doesn't already have a room added
    if (!roomNodeGraph.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count > 0)
    {
      return false;
    }

    return true;
  }

  /// <summary>
  /// Add parentID to the node (returns true if the node has been added, false otherwise)
  /// </summary>
  public bool AddParentRoomNodeIDToRoomNode(string parentID)
  {
    parentRoomNodeIDList.Add(parentID);
    return true;
  }

#endif

  #endregion Editor Code
}