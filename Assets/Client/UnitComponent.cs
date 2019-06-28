using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitComponent : MonoBehaviour
{
  public GameObject teamColorObject;
  public Animator animator;
  public Unit unit;

  public void Initialize()
  {
    var renderer = this.teamColorObject.GetComponent<Renderer>();
    var color = unit.player.id == 0 ? Color.red : Color.blue;
    renderer.material.SetColor("_TeamColor", color);
  }

  void Update()
  {
    this.transform.localScale = new Vector3(1, 1, 1) * ((this.unit.player.upgrade * .1f) + 1);
    this.MoveAndRotate();
    this.PlayAnimation();
  }

  void MoveAndRotate()
  {
    Vector3 direction = Vector3.zero;
    if (this.unit.attacking) direction = VectorUtil.Vector2To3(this.unit.attackTarget.position - this.unit.position, 0);
    else
    {
      Vector3 lastPosition = this.transform.position;
      this.transform.position = VectorUtil.Vector2To3(this.unit.position, this.unit.height);
      direction = (this.transform.position - lastPosition).normalized;
    }
    if (direction != Vector3.zero) this.RotateToDirection(direction);
  }

  void RotateToDirection(Vector3 direction)
  {
    this.transform.forward = Vector3.RotateTowards(this.transform.forward, direction, 50f * Time.deltaTime, 0f);
  }

  void PlayAnimation()
  {
    if (this.unit.attacking) this.animator.Play("Attack");
    else this.animator.Play("Idle");
  }
}