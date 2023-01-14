using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings
{
  #region DUNGEON BUILD SETTINGS
  public const int maxDungeonRebuildAttemptsForRoomGraph = 1000;
  public const int maxDungeonBuildAttempts = 10;
  #endregion


  #region Room Settings
  // Max number of child corridors leading from a room - maximum should be 3 although this is not recommended since it can cause the dungeon building to fail as the rooms are more likely to NOT fit together.
  public const int maxChildCorridors = 3;
  #endregion

}
