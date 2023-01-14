using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResources : MonoBehaviour
{
  private static GameResources instance;

  public static GameResources Instance
  {
    get
    {
      if (instance == null)
      {
        instance =
            Resources.Load<GameResources>("GameResources");
      }

      return instance;
    }

    // Not in the tutorial
    private set { }
  }

  #region Header DUNGEON
  [Space(10)]
  [Header("Dungeon")]
  #endregion
  #region Tooltip
  [Tooltip("Populate with the dungeon RoomNodeTypeListSO")]
  #endregion

  public RoomNodeTypeListSO roomNodeTypeList;

  #region Header MATERIALS
  [Space(10)]
  [Header("Materials")]
  #endregion
  #region Tooltip
  [Tooltip("Dimmed Material")]
  #endregion

  public Material dimmedMaterial;
}
