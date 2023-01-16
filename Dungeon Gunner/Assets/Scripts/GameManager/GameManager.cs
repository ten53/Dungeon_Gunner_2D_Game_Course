using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehavior<GameManager>
{
  #region Header DUNGEON LEVELS
  [Space(10)]
  [Header("DUNGEON LEVELS")]
  #endregion Header DUNGEON LEVELS
  #region Tooltip
  [Tooltip("Populate with the dungeon level scriptable objects")]
  #endregion Tooltip
  [SerializeField]
  private List<DungeonLevelSO> dungeonLevelList;

  #region Tooltip
  [Tooltip("Populate with the dungeon level for testing, first level = 0")]
  #endregion Tooltip
  [SerializeField]
  private int currentDungeonLevelListIndex = 0;

  [HideInInspector]
  public GameState gameState;

  private void Start()
  {
    gameState = GameState.gameStarted;
  }

  private void Update()
  {
    HandleGameState();

    // For testing only - press R to restart
    if (Input.GetKeyDown(KeyCode.R))
    {
      gameState = GameState.gameStarted;
    }
  }

  /// <summary>
  /// Handle different game states
  /// </summary>
  private void HandleGameState()
  {
    switch (gameState)
    {
      case GameState.gameStarted:
        // Play first level
        PlayDungeonLevel(currentDungeonLevelListIndex);
        gameState = GameState.playingLevel;
        break;
    }
  }

  /// <summary>
  /// Pass dungeon level index into dungeon builder
  /// </summary>
  private void PlayDungeonLevel(int dungeonLevelListIndex)
  {
    // Build dungeon for level
    bool dungeonBuildSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

    if (!dungeonBuildSuccessfully)
    {
      Debug.Log("Couldn't build dungeon from specified rooms and node graphs");
    }
  }

  #region Validation
#if UNITY_EDITOR
  void OnValidate()
  {
    HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
  }
#endif
  #endregion

}
