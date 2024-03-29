using System;
using UnityEngine;

public class UnitComponent : MonoBehaviour
{
  public int health;
  public GameObject teamColorObject;
  public GameComponent gameComponent;
  public HealthBarComponent healthBar;
  public Animator animator;
  public Unit unit;
  Vector2 lastPosition = Vector2.zero;
  bool initialized = false;
  bool lastInvisible = false;

  public void Initialize()
  {
    this.unit.OnAttack.Add(() => this.animator.Play("Attack"));
    var renderer = this.teamColorObject.GetComponent<Renderer>();
    var color = unit.player.id == 0 ? Color.red : Color.blue;
    // renderer.material.SetColor("_TeamColor", color);
    renderer.material.color = color;
    if (this.unit.speed != 0f)
    {
      this.transform.forward = VectorUtil.Vector2To3(this.unit.target - this.unit.position, 0f);
    }
    this.lastPosition = this.unit.position;
    this.initialized = true;
  }

  void Update()
  {
    if (!this.initialized) return;
    this.health = this.unit.health;
    this.ApplyInvisibility();
    this.healthBar.OnHealthChanged(1f * this.unit.health / this.unit.maxHealth);
    this.transform.localScale = new Vector3(1, 1, 1) * ((this.unit.player.upgrade * .1f) + 1);
    this.MoveAndRotate();
  }

  void ApplyInvisibility()
  {
    if (this.unit.invisible == this.lastInvisible) return;
    var a = this.unit.invisible ? (this.unit.player.id == this.gameComponent.id ? .3f : .05f) : 1f;
    Array.ForEach(gameObject.GetComponentsInChildren<Renderer>(), renderer =>
    {
      if (this.unit.invisible) StandardShaderUtils.ChangeRenderMode(renderer.material, StandardShaderUtils.BlendMode.Fade);
      else StandardShaderUtils.ChangeRenderMode(renderer.material, StandardShaderUtils.BlendMode.Opaque);
      var color = renderer.material.color;
      color.a = a;
      renderer.material.color = color;
    });
    this.lastInvisible = this.unit.invisible;
  }

  void MoveAndRotate()
  {
    var direction = Vector3.zero;
    if (this.unit.attacking) direction = VectorUtil.Vector2To3(this.unit.attackTarget.position - this.unit.position, 0);
    else if (this.DidUnitMove())
    {
      this.transform.position = VectorUtil.Vector2To3(this.unit.position, this.unit.height);
      direction = VectorUtil.Vector2To3(this.unit.position - this.lastPosition).normalized;
      this.lastPosition = this.unit.position;
      this.animator.Play("Idle");
    }
    this.RotateToDirection(direction);
  }

  void RotateToDirection(Vector3 direction)
  {
    if (direction == Vector3.zero) return;
    this.transform.forward = Vector3.RotateTowards(this.transform.forward, direction, 50f * Time.deltaTime, 0f);
  }

  bool DidUnitMove()
  {
    return !this.unit.position.Equals(this.lastPosition);
  }
}