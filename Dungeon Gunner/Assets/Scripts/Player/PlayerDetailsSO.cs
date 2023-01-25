using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDetails_", menuName = "Scriptable Objects/Player/Player Details")]
public class PlayerDetailsSO : ScriptableObject
{
  #region Header PLAYER BASE DETAILS
  [Space(10)]
  [Header("PLAYER BASE DETAILS")]
  #endregion
  #region Tooltip
  [Tooltip("Player character name")]
  #endregion
  public string playerCharacterName;

  #region Tooltip
  [Tooltip("Prefab gameobject for the player")]
  #endregion
  public GameObject playerPrefab;

  #region Tooltip
  [Tooltip("Player runtime animator controller")]
  #endregion
  public RuntimeAnimatorController runtimeAnimatorController;

  #region Header HEALTH
  [Space(10)]
  [Header("HEALTH")]
  #endregion
  #region Tooltip
  [Tooltip("Player starting health amount")]
  #endregion
  public int playerHealthAmount;

  #region Header OTHER
  [Space(10)]
  [Header("OTHER")]
  #endregion
  #region Tooltip
  [Tooltip("Player icon sprite to be used in the minimap")]
  #endregion
  public Sprite playerMiniMapIcon;

  #region Tooltip
  [Tooltip("Player hande sprite")]
  #endregion
  public Sprite playerHandSprite;

  #region Validation
#if UNITY_EDITOR
  /// <summary>
  /// Called when the script is loaded or a value is changed in the
  /// inspector (Called in the editor only).
  /// </summary>
  private void OnValidate()
  {
    HelperUtilities.ValidateCheckEmptyString(this, nameof(playerCharacterName), playerCharacterName);
    HelperUtilities.ValidateCeckNullValue(this, nameof(playerPrefab), playerPrefab);
    HelperUtilities.ValidatePositiveValue(this, nameof(playerHealthAmount), playerHealthAmount, false);
    HelperUtilities.ValidateCeckNullValue(this, nameof(playerMiniMapIcon), playerMiniMapIcon);
    HelperUtilities.ValidateCeckNullValue(this, nameof(playerHandSprite), playerHandSprite);
    HelperUtilities.ValidateCeckNullValue(this, nameof(runtimeAnimatorController), runtimeAnimatorController);
  }
#endif
  #endregion
}
