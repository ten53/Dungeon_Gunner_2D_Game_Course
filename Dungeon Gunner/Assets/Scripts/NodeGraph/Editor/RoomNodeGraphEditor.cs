using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;


public class RoomNodeGraphEditor : EditorWindow
{
  private GUIStyle roomNodeStyle;
  private GUIStyle roomNodeSelectedStyle;
  private static RoomNodeGraphSO currentRoomNodeGraph;
  private RoomNodeSO currentRoomNode = null;
  private RoomNodeTypeListSO roomNodeTypeList;

  // Node layout values
  private const float nodeWidth = 160f;
  private const float nodeHeight = 75f;
  private const int nodePadding = 25;
  private const int nodeBorder = 12;

  // Connecting line values
  private const float connectingLineWidth = 3f;
  private const float connectingLineArrowSize = 6f;

  [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/ Room Node Graph Editor")]
  private static void OpenWindow()
  {
    GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
  }

  /// <summary>
  /// This function is called when the object becomes enabled and active.
  /// </summary>
  private void OnEnable()
  {
    // Subscribe to the inspector selection changed event
    Selection.selectionChanged += InspectorSelectionChanged;

    //Todo: refactor node layout style (standard / selected) into function that takes background style (e.g. "node1 or "node1 on" ) as argument
    // Define standard node layout style
    roomNodeStyle = new GUIStyle();
    roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
    roomNodeStyle.normal.textColor = Color.white;
    roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
    roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

    // Define selected node layout style
    roomNodeSelectedStyle = new GUIStyle();
    roomNodeStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
    roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
    roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

    // Load room node types
    roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
  }

  /// <summary>
  /// This function is called when the behaviour becomes disabled or inactive.
  /// </summary>
  private void OnDisable()
  {
    // Unsubscribe from the inspector selection changes event
    Selection.selectionChanged -= InspectorSelectionChanged;
  }

  /// <summary>
  /// Open the room node graph editor window if a room node graph scriptable object asset is double clicked in the inspector
  /// </summary>
  [OnOpenAsset(0)]
  public static bool OnDoubleClickAssets(int instanceID, int line)
  {
    RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;

    if (roomNodeGraph != null)
    {
      OpenWindow();

      currentRoomNodeGraph = roomNodeGraph;

      return true;
    }

    return false;
  }

  /// <summary>
  /// Draw Editor GUI
  /// </summary>
  private void OnGUI()
  {
    // If a scriptable object of type RoomNodeGraphSO has been selected then process
    if (currentRoomNodeGraph != null)
    {
      // Draw line if being dragged - called before drawing room nodes to not appear on top of room nodes
      DrawDraggedLine();

      // Process Events
      ProcessEvents(Event.current);

      // Draw connections between room nodes
      DrawRoomNodeConnections();

      // Draw Room Nodes
      DrawRoomNodes();
    }

    if (GUI.changed) Repaint();
  }

  private void DrawDraggedLine()
  {
    if (currentRoomNodeGraph.linePosition != Vector2.zero)
    {
      // Draw line from node to lineposition - (startPosition, endPosition, startTangent, endTangent, color, texture, width)
      Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);

    }
  }

  private void ProcessEvents(Event currentEvent)
  {
    // Get room node that mouse is over if it's null or not currently being dragged
    if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
    {
      currentRoomNode = IsMouseOverRoomNode(currentEvent);
    }

    // if mouse isn't over a room node
    if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
    {
      ProcessRoomNodeGraphEvents(currentEvent);
    }
    // else process room node events
    else
    {
      currentRoomNode.ProcessEvents(currentEvent);
    }
  }

  /// <summary>
  /// Check to see if mouse is over a room node - if so then return thr room node else return null
  /// </summary>
  private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
  {
    for (int i = currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
    {
      if (currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
      {
        return currentRoomNodeGraph.roomNodeList[i];
      }
    }

    return null;
  }

  /// <summary>
  /// Process Room Node Graph Events
  /// </summary>
  private void ProcessRoomNodeGraphEvents(Event currentEvent)
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

      // Process mouse drag event
      case EventType.MouseDrag:
        ProcessMouseDragEvent(currentEvent);
        break;

      default:
        break;
    }
  }

  /// <summary>
  /// Process mouse down events on the room node graph (not over a node)
  /// </summary>
  private void ProcessMouseDownEvent(Event currentEvent)
  {
    // Process right click mouse down on graph event (show context menu)
    if (currentEvent.button == 1)
    {
      ShowContextMenu(currentEvent.mousePosition);
    }
    //Process left mouse down on graph event
    else if (currentEvent.button == 0)
    {
      ClearLineDrag();
      ClearAllSelectedRoomNodes();
    }
  }

  /// <summary>
  /// Show the context menu
  /// </summary>
  private void ShowContextMenu(Vector2 mousePosition)
  {
    GenericMenu menu = new GenericMenu();

    menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

    menu.ShowAsContext();
  }

  /// <summary>
  /// Create a room node at the mouse position
  /// </summary>
  private void CreateRoomNode(object mousePositionObject)
  {
    // If current node graph empty then add entrance room node first
    if (currentRoomNodeGraph.roomNodeList.Count == 0)
    {
      CreateRoomNode(new Vector2(200f, 200f), roomNodeTypeList.list.Find(x => x.isEntrance));
    }

    CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isNone));
  }

  /// <summary>
  /// Create a room node at the mouse position - overloaded to also pass in RoomNodeType
  /// </summary>
  private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
  {
    Vector2 mousePosition = (Vector2)mousePositionObject;

    // create room node scriptable object asset
    RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

    // add room node scriptable object asset
    currentRoomNodeGraph.roomNodeList.Add(roomNode);

    // set room node values
    roomNode.Initialize(new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight)), currentRoomNodeGraph, roomNodeType);

    // add room node to room node graph scriptable object asset database
    AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);

    AssetDatabase.SaveAssets();

    // Refresh graph node dictionary
    currentRoomNodeGraph.OnValidate();
  }

  /// <summary>
  /// Clear selection from all room nodes
  /// </summary>
  private void ClearAllSelectedRoomNodes()
  {
    foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
    {
      if (roomNode.isSelected)
      {
        roomNode.isSelected = false;

        GUI.changed = true;
      }
    }
  }

  /// <summary>
  /// Process mouse up events
  /// </summary>
  private void ProcessMouseUpEvent(Event currentEvent)
  {
    // if releasing the right mouse button and currently dragging a line
    if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
    {

      // Check if over a room node
      RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);

      if (roomNode != null)
      {
        // if so set it as a child of the parent room node if it can be added
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
        {
          // Set parent ID in child room node
          roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
        }
      }

      ClearLineDrag();
    }
  }

  /// <summary>
  /// Process mouse drag event
  /// </summary>
  private void ProcessMouseDragEvent(Event currentEvent)
  {
    // process right click drag event - draw line
    if (currentEvent.button == 1)
    {
      ProcessRightMouseDragEvent(currentEvent);
    }
  }

  /// <summary>
  /// Process right mouse drag event - draw line
  /// </summary>
  private void ProcessRightMouseDragEvent(Event currentEvent)
  {
    if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
    {
      DragConnectingLine(currentEvent.delta);
      GUI.changed = true;
    }
  }

  /// <summary>
  /// Drag connecting line from room node
  /// </summary>
  private void DragConnectingLine(Vector2 delta)
  {
    currentRoomNodeGraph.linePosition += delta;
  }

  /// <summary>
  /// Clear line drag from a room node
  /// </summary>
  private void ClearLineDrag()
  {
    currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
    currentRoomNodeGraph.linePosition = Vector2.zero;
    GUI.changed = true;
  }

  /// <summary>
  /// Draw connections in the graph window between room nodes
  /// </summary>
  private void DrawRoomNodeConnections()
  {
    // Loop through all room nodes
    foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
    {
      if (roomNode.childRoomNodeIDList.Count > 0)
      {
        // Loop through child room nodes
        foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
        {
          // get child room node from dictionary
          if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
          {
            DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);

            GUI.changed = true;
          }
        }
      }
    }
  }

  /// <summary>
  /// Draw connection line between the parent room node and child room node
  /// </summary>
  private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
  {
    // get line start and end position
    Vector2 startPosition = parentRoomNode.rect.center;
    Vector2 endPosition = childRoomNode.rect.center;

    // Calculate midway point
    Vector2 midPosition = (endPosition + startPosition) / 2f;

    // Vector from start to end position of line
    Vector2 direction = endPosition - startPosition;

    // Calculate normalized perpendicular positions from the mid point
    Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
    Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

    // Calculate mid point offset position for arrow head
    Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

    // Draw arrow
    Handles.DrawBezier(arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, Color.white, null, connectingLineWidth);
    Handles.DrawBezier(arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, Color.white, null, connectingLineWidth);

    // Draw line
    Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

    GUI.changed = true;
  }

  /// <summary>
  /// Draw room nodes in the graph window
  /// </summary>
  private void DrawRoomNodes()
  {
    //todo: bugfix - standard room node style does not get applied after deselecting room node - background just disappears
    // Loop through all room nodes and draw them
    foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
    {
      if (roomNode.isSelected)
      {
        roomNode.Draw(roomNodeSelectedStyle);
      }
      else
      {
        roomNode.Draw(roomNodeStyle);
      }
    }

    GUI.changed = true;
  }

  /// <summary>
  /// Selection changed in the inspector
  /// </summary>
  private void InspectorSelectionChanged()
  {
    RoomNodeGraphSO roomNodeGraph = Selection.activeObject as RoomNodeGraphSO;

    if (roomNodeGraph != null)
    {
      currentRoomNodeGraph = roomNodeGraph;
      GUI.changed = true;
    }
  }
}