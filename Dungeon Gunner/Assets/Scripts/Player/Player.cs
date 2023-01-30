using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
#endregion

public class Player : MonoBehaviour
{
  [HideInInspector] public PlayerDetailsSO playerDetails;
  [HideInInspector] public Health health;
  [HideInInspector] public SpriteRenderer spriteRenderer;
  [HideInInspector] public Animator animator;

  void Awake()
  {
    // Load Components
    health = GetComponent<Health>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    animator = GetComponent<Animator>();
  }

  /// <summary>
  /// Initialize the player
  /// </summary>
  public void Iniitialize(PlayerDetailsSO playerDetails)
  {
    this.playerDetails = playerDetails;

    // Set player health
    SetPlayerHealth();
  }

  /// <summary>
  /// Set player health from playerDetails SO
  /// </summary>
  private void SetPlayerHealth()
  {
    health.SetStartingHealth(playerDetails.playerHealthAmount);
  }
}